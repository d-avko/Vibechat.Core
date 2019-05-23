using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.ChatData.Messages;
using Vibechat.Web.Data.ApiModels.Conversation;
using Vibechat.Web.Data.ApiModels.Messages;
using Vibechat.Web.Services.ChatDataProviders;
using Vibechat.Web.Services.FileSystem;
using Vibechat.Web.Services.Repositories;
using VibeChat.Web;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Services
{
    public class ConversationsInfoService
    {
        public ConversationsInfoService(
            IChatDataProvider chatDataProvider,
            IUsersRepository usersRepository,
            IMessagesRepository messagesRepository,
            IAttachmentRepository attachmentRepository,
            IAttachmentKindsRepository attachmentKindsRepository,
            IUsersConversationsRepository usersConversationsRepository,
            IConversationRepository conversationRepository,
            ImagesService imagesService)
        {
            this.chatDataProvider = chatDataProvider;
            this.usersRepository = usersRepository;
            this.messagesRepository = messagesRepository;
            this.attachmentRepository = attachmentRepository;
            this.attachmentKindsRepository = attachmentKindsRepository;
            this.usersConversationsRepository = usersConversationsRepository;
            this.conversationRepository = conversationRepository;
            ImagesService = imagesService;
        }

        protected readonly IChatDataProvider chatDataProvider;

        protected readonly IUsersRepository usersRepository;

        protected readonly IMessagesRepository messagesRepository;

        protected readonly IAttachmentRepository attachmentRepository;

        protected readonly IAttachmentKindsRepository attachmentKindsRepository;

        protected readonly IUsersConversationsRepository usersConversationsRepository;

        protected readonly IConversationRepository conversationRepository;

        private const int MaxThumbnailLengthMB = 5;

        private const int MaxNameLength = 200;

        protected readonly ImagesService ImagesService;

        #region Conversations

        public async Task<ConversationTemplate> CreateConversation(CreateConversationCredentialsApiModel convInfo)
        {

            var defaultError = new FormatException("Error while creating the conversation..");

            //if there was no group info or creator info

            if (string.IsNullOrWhiteSpace(convInfo.ConversationName) || string.IsNullOrWhiteSpace(convInfo.CreatorId))
            {
                throw defaultError;
            }

            var user = await usersRepository.GetById(convInfo.CreatorId).ConfigureAwait(false);

            if (user == null)
            {
                throw defaultError;
            }

            if ((!convInfo.IsGroup) && (convInfo.DialogUserId == null))
            {
                throw new FormatException("No dialogue user was provided...");
            }

            UserInApplication SecondDialogueUser = null;

            if (convInfo.DialogUserId != null)
            {
                //if this is a dialogue , find a user with whom to create conversation
                SecondDialogueUser = await usersRepository.GetById(convInfo.DialogUserId);

                if (SecondDialogueUser == null)
                {
                    throw defaultError;
                }
            }

            ConversationDataModel ConversationToAdd = await conversationRepository.Add(
                convInfo.IsGroup,
                convInfo.IsGroup ? convInfo.ConversationName : null,
                convInfo.ImageUrl ?? chatDataProvider.GetGroupPictureUrl(),
                user,
                convInfo.IsPublic
                );

            await usersConversationsRepository.Add(user, ConversationToAdd);

            //after saving changes we have Id of our 
            //created conversation in ConversationToAdd variable

            return  new ConversationTemplate()
                    {
                        ConversationID = ConversationToAdd.ConvID,
                        DialogueUser = (ConversationToAdd.IsGroup) ?
                          null :
                          new UserInfo()
                          {
                              Id = SecondDialogueUser.Id,
                              Name = SecondDialogueUser.FirstName,
                              ImageUrl = SecondDialogueUser.ProfilePicImageURL,
                              LastName = SecondDialogueUser.LastName,
                              LastSeen = SecondDialogueUser.LastSeen,
                              UserName = SecondDialogueUser.UserName,
                              ConnectionId = SecondDialogueUser.ConnectionId,
                              IsOnline = SecondDialogueUser.IsOnline
                          },

                        ThumbnailUrl = ConversationToAdd.ThumbnailUrl,
                        FullImageUrl = ConversationToAdd.FullImageUrl,
                        IsGroup = ConversationToAdd.IsGroup,
                        Name = ConversationToAdd.Name,
                        Participants = (await GetConversationParticipants(new GetParticipantsApiModel() { ConvId = ConversationToAdd.ConvID })).Participants,
                        Messages = null,
                        Creator = new UserInfo()
                        {
                            Id = ConversationToAdd.Creator.Id,
                            Name = ConversationToAdd.Creator.FirstName,
                            ImageUrl = ConversationToAdd.Creator.ProfilePicImageURL,
                            LastName = ConversationToAdd.Creator.LastName,
                            LastSeen = ConversationToAdd.Creator.LastSeen,
                            UserName = ConversationToAdd.Creator.UserName,
                            ConnectionId = ConversationToAdd.Creator.ConnectionId,
                            IsOnline = ConversationToAdd.Creator.IsOnline
                        }
                    };

        }

        public async Task<UpdateThumbnailResponse> UpdateThumbnail(int conversationId, IFormFile image)
        {
            if ((image.Length / (1024 * 1024)) > MaxThumbnailLengthMB)
            {
                throw new InvalidDataException($"Thumbnail was larger than {MaxThumbnailLengthMB}");
            }

            ConversationDataModel conversation = conversationRepository.GetById(conversationId);

            if(conversation == null)
            {
                throw new InvalidDataException($"Wrong conversation id was provided.");
            }

            using (var buffer = new MemoryStream())
            {
                image.CopyTo(buffer);
                buffer.Seek(0, SeekOrigin.Begin);
                var thumbnailFull = ImagesService.UpdateConversationThumbnail(buffer, image.FileName);

                conversationRepository.UpdateThumbnail(thumbnailFull.Item1, thumbnailFull.Item2, conversation);
            }

            return new UpdateThumbnailResponse()
            {
                ThumbnailUrl = conversation.ThumbnailUrl,
                FullImageUrl = conversation.FullImageUrl
            };
        }

        public async Task ChangeName (int conversationId, string name)
        {
            if(name.Length > MaxNameLength)
            {
                throw new InvalidDataException($"Name length couldn't be more than {MaxNameLength}.");
            }

            var conversation = conversationRepository.GetById(conversationId);

            if (conversation == null)
            {
                throw new InvalidDataException($"Wrong conversation id was provided.");
            }

            conversationRepository.ChangeName(conversation, name);
        }

        public async Task<List<ConversationTemplate>> SearchForGroups(string name, string whoAccessedId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidDataException($"Search string coudn't be null.");
            }

            var groups = await conversationRepository.SearchByName(name);

            if(groups == null)
            {
                return null;
            }

            var result = new List<ConversationTemplate>();

            foreach (var conversation in groups)
            {
                result.Add(
                    new ConversationTemplate()
                    {
                        Name = conversation.Name,
                        ConversationID = conversation.ConvID,
                        DialogueUser = null,
                        IsGroup = conversation.IsGroup,
                        ThumbnailUrl = conversation.ThumbnailUrl,
                        FullImageUrl = conversation.FullImageUrl,
                        Participants = (await GetConversationParticipants(new GetParticipantsApiModel() { ConvId = conversation.ConvID })).Participants,

                        //only get last message here, client should fetch messages after he opened the conversation.

                        Messages = (await GetConversationMessages(new GetMessagesApiModel() { ConversationID = conversation.ConvID, Count = 1, MesssagesOffset = 0 }, whoAccessedId)).Messages,
                        Creator = new UserInfo()
                        {
                            Id = conversation.Creator.Id,
                            Name = conversation.Creator.FirstName,
                            ImageUrl = conversation.Creator.ProfilePicImageURL,
                            LastName = conversation.Creator.LastName,
                            LastSeen = conversation.Creator.LastSeen,
                            UserName = conversation.Creator.UserName,
                            ConnectionId = conversation.Creator.ConnectionId,
                            IsOnline = conversation.Creator.IsOnline
                        }
                    }
                    );
            }

            return result;
        }


        public async Task RemoveUserFromConversation(string userId, string whoRemovedId, int conversationId, bool IsSelf)
        {
            if(!IsSelf && userId == whoRemovedId)
            {
                throw new FormatException("Couldn't remove yourself from conversation.");
            }

            var conversation = conversationRepository.GetById(conversationId);

            if(conversation == null)
            {
                throw new FormatException("Wrong conversation.");
            }

            var userConversation = await usersConversationsRepository.Get(userId, conversationId);

            if (userConversation == null)
            {
                throw new FormatException("User is not a part of this conversation.");
            }

            if(conversation.Creator.Id != whoRemovedId && !IsSelf)
            {
                throw new FormatException("Only creator can remove users.");
            }

            // conversation could only exist if there are some users in it.

            if ((await GetConversationParticipants(new GetParticipantsApiModel() { ConvId = conversationId }))
                .Participants
                .Count() == 1)
            {
                await usersConversationsRepository.Remove(userConversation);
                conversationRepository.Remove(conversation);
                return;
            }
            else
            {
                await usersConversationsRepository.Remove(userConversation);
            }
        }


        public async Task<UserInfo> AddUserToConversation(AddToConversationApiModel UserProvided)
        {
            //CHECK IF THIS IS EVEN ALLOWED

            var defaultError = new FormatException("Invalid credentials were provided.");

            var FoundConversation = conversationRepository.GetById(UserProvided.ConvId);

            if (FoundConversation == null)
                throw defaultError;

            var FoundUser = await usersRepository.GetById(UserProvided.UserId).ConfigureAwait(false);

            if (FoundUser == null)
                throw defaultError;

            if(await usersConversationsRepository.Exists(FoundUser.Id, FoundConversation.ConvID))
            {
                throw new FormatException("User already exists in converation.");
            }

            var addedUser = (await usersConversationsRepository.Add(FoundUser, FoundConversation)).User;

            return new UserInfo()
            {
                Id = addedUser.Id,
                Name = addedUser.FirstName,
                ImageUrl = addedUser.ProfilePicImageURL,
                LastName = addedUser.LastName,
                LastSeen = addedUser.LastSeen,
                UserName = addedUser.UserName,
                ConnectionId = addedUser.ConnectionId,
                IsOnline = addedUser.IsOnline
            };
        }

        public async Task<ConversationInfoResultApiModel> GetConversationInformation(CredentialsForConversationInfoApiModel UserProvided, string whoAccessedId)
        {
            var defaultError = new FormatException("User info provided was not correct.");

            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            if (UserProvided.UserId != whoAccessedId)
            {
                throw unAuthorizedError;
            }

            if (UserProvided == null)
                throw defaultError;

            if (UserProvided.UserId == null)
                throw defaultError;

            var user = await usersRepository.GetById(UserProvided.UserId).ConfigureAwait(false);

            if (user == null)
                throw defaultError;


            IQueryable<ConversationDataModel> conversations = usersConversationsRepository.GetUserConversations(UserProvided.UserId);

            var returnData = new List<ConversationTemplate>();

            foreach (ConversationDataModel conversation in conversations)
            {
                UserInApplication DialogueUser = conversation.IsGroup ? null : usersConversationsRepository.GetUserInDialog(conversation.ConvID, UserProvided.UserId);

                returnData.Add
                    (
                        new ConversationTemplate()
                        {
                            Name = conversation.IsGroup ? conversation.Name : DialogueUser.UserName,
                            ConversationID = conversation.ConvID,
                            DialogueUser = DialogueUser == null ?
                            null
                            :
                            new UserInfo()
                            {
                                Id = DialogueUser.Id,
                                Name = DialogueUser.FirstName,
                                LastName = DialogueUser.LastName,
                                LastSeen = DialogueUser.LastSeen,
                                UserName = DialogueUser.UserName,
                                ImageUrl = DialogueUser.ProfilePicImageURL,
                                IsOnline = DialogueUser.IsOnline,
                                ConnectionId = DialogueUser.ConnectionId
                            },
                            IsGroup = conversation.IsGroup,
                            ThumbnailUrl = conversation.ThumbnailUrl,
                            FullImageUrl = conversation.FullImageUrl,
                            Participants = (await GetConversationParticipants(new GetParticipantsApiModel() { ConvId = conversation.ConvID })).Participants,

                            //only get last message here, client should fetch messages after he opened the conversation.

                            Messages = (await GetConversationMessages(new GetMessagesApiModel() { ConversationID = conversation.ConvID, Count = 1, MesssagesOffset = 0 }, whoAccessedId)).Messages,
                            Creator = new UserInfo()
                            {
                                Id = conversation.Creator.Id,
                                Name = conversation.Creator.FirstName,
                                ImageUrl = conversation.Creator.ProfilePicImageURL,
                                LastName = conversation.Creator.LastName,
                                LastSeen = conversation.Creator.LastSeen,
                                UserName = conversation.Creator.UserName,
                                ConnectionId = conversation.Creator.ConnectionId,
                                IsOnline = conversation.Creator.IsOnline
                            }
                        }
                    );
            }

            return new ConversationInfoResultApiModel()
            {
                Conversations = returnData
            };

        }


        public async Task<GetParticipantsResultApiModel> GetConversationParticipants(GetParticipantsApiModel convInfo)
        {
            var defaultErrorMessage = new FormatException("Wrong conversation was provided.");

            if (convInfo == null)
                throw defaultErrorMessage;

            var conversation = conversationRepository.GetById(convInfo.ConvId);

            if (conversation == null)
                throw defaultErrorMessage;

            var participants = usersConversationsRepository.GetConversationParticipants(convInfo.ConvId);
            
            return new GetParticipantsResultApiModel()
            {
                Participants = (from participant in participants
                               select new UserInfo()
                               {
                                   Id = participant.Id,
                                   LastName = participant.LastName,
                                   LastSeen = participant.LastSeen,
                                   Name = participant.FirstName,
                                   UserName = participant.UserName,
                                   ImageUrl = participant.ProfilePicImageURL,
                                   ConnectionId = participant.ConnectionId,
                                   IsOnline = participant.IsOnline
                               }).ToList()
             };

        }


        public async Task<GetMessagesResultApiModel> GetConversationMessages(GetMessagesApiModel convInfo, string whoAccessedId)
        {
            var defaultErrorMessage = new FormatException("Wrong conversation was provided.");

            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            if (messagesRepository.Empty())
            {
                return new GetMessagesResultApiModel()
                {
                    Messages = null
                };
            }

            if (convInfo == null)
                throw defaultErrorMessage;

            var conversation = conversationRepository.GetById(convInfo.ConversationID);

            if (conversation == null)
                throw defaultErrorMessage;

            var members = usersConversationsRepository.GetConversationParticipants(convInfo.ConversationID);

            //only member of conversation could request messages of non-public conversation.

            if (members.FirstOrDefault(x => x.Id == whoAccessedId) == null && !conversation.IsPublic)
                throw unAuthorizedError;

            var messages = messagesRepository.GetMessagesForConversation(
                whoAccessedId,
                convInfo.ConversationID,
                convInfo.MesssagesOffset,
                convInfo.Count);

            return new GetMessagesResultApiModel()
            {
                Messages = (from msg in messages
                            select new Message()
                            {
                                Id = msg.MessageID,
                                ConversationID = msg.ConversationID,
                                MessageContent = msg.MessageContent,
                                TimeReceived = msg.TimeReceived,
                                User = msg.User == null ? null : new UserInfo()
                                {
                                    Id = msg.User.Id,
                                    LastName = msg.User.LastName,
                                    LastSeen = msg.User.LastSeen,
                                    Name = msg.User.FirstName,
                                    UserName = msg.User.UserName,
                                    ImageUrl = msg.User.ProfilePicImageURL,
                                    IsOnline = msg.User.IsOnline,
                                    ConnectionId = msg.User.ConnectionId
                                },
                                AttachmentInfo = msg.AttachmentInfo == null ? null : new MessageAttachment()
                                {
                                    AttachmentKind = msg.AttachmentInfo.AttachmentKind.Name,
                                    ContentUrl = msg.AttachmentInfo.ContentUrl,
                                    AttachmentName = msg.AttachmentInfo.AttachmentName,
                                    ImageHeight = msg.AttachmentInfo.ImageHeight,
                                    ImageWidth = msg.AttachmentInfo.ImageWidth
                                },
                                IsAttachment = msg.IsAttachment
                            }).ToList()
            };

        }


        public async Task<GetConversationByIdResultApiModel> GetConversationById(GetConversationByIdApiModel convInfo, string whoAccessedId)
        {

            ConversationDataModel conversation = conversationRepository.GetById(convInfo.ConversationId);

            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            var user = new UserInApplication();

            if (conversation.IsGroup)
            {
                user = await usersRepository.GetById(whoAccessedId).ConfigureAwait(false);
            }


            if (conversation == null)
            {
                throw new FormatException("No such conversation was found.");
            }

            var members = usersConversationsRepository.GetConversationParticipants(convInfo.ConversationId);

            //only member of conversation could request messages (there should exist other calls for non-members).

            if (members.FirstOrDefault(x => x.UserName == whoAccessedId) == null)
                throw unAuthorizedError;

            if (conversation.IsGroup && user == null)
            {
                throw new FormatException("User with such id was not found.");
            }

            user = usersConversationsRepository.GetUserInDialog(convInfo.ConversationId, user.Id);

            if (user == null)
            {
                throw new FormatException("Unexpected error: no corresponding user in dialogue.");
            }

            return new GetConversationByIdResultApiModel()
            {
                Conversation = new ConversationTemplate()
                {
                    ConversationID = conversation.ConvID,
                    DialogueUser = (conversation.IsGroup) ? null : new UserInfo()
                    {
                        Id = user.Id,
                        Name = user.FirstName,
                        ImageUrl = user.ProfilePicImageURL,
                        LastName = user.LastName,
                        LastSeen = user.LastSeen,
                        UserName = user.UserName,
                        IsOnline = user.IsOnline,
                        ConnectionId = user.ConnectionId
                    },
                    ThumbnailUrl = conversation.ThumbnailUrl,
                    FullImageUrl = conversation.FullImageUrl,
                    IsGroup = conversation.IsGroup,
                    Name = conversation.Name,
                    Creator = new UserInfo()
                    {
                        Id = conversation.Creator.Id,
                        Name = conversation.Creator.FirstName,
                        ImageUrl = conversation.Creator.ProfilePicImageURL,
                        LastName = conversation.Creator.LastName,
                        LastSeen = conversation.Creator.LastSeen,
                        UserName = conversation.Creator.UserName,
                        ConnectionId = conversation.Creator.ConnectionId,
                        IsOnline = conversation.Creator.IsOnline
                    }
                }

            };

        }

        public async Task<MessageDataModel> AddMessage(Message message, int groupId, string SenderId)
        {
            UserInApplication whoSent = await usersRepository.GetById(SenderId);

            if (whoSent == null)
            {
                throw new FormatException($"Failed to retrieve user with id {SenderId} from database: no such user exists");
            }

            return await messagesRepository.Add(whoSent, message, groupId);
        }

        public async Task<MessageDataModel> AddAttachmentMessage(Message message, int groupId, string SenderId)
        {
            UserInApplication whoSent = await usersRepository.GetById(SenderId);

            if (whoSent == null)
            {
                throw new FormatException($"Failed to retrieve user with id {SenderId} from database: no such user exists");
            }

            AttachmentKindDataModel attachmentKind = await attachmentKindsRepository.GetById(message.AttachmentInfo.AttachmentKind);

            MessageAttachmentDataModel attachment = await attachmentRepository.Add(attachmentKind, message);

            return await messagesRepository.AddAttachment(whoSent, attachment, message, groupId, SenderId);
        }

        public async Task DeleteConversationMessages(DeleteMessagesRequest messagesInfo, string whoAccessedId)
        {
            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            IQueryable<MessageDataModel> messagesToDelete = messagesRepository.GetMessagesByIds(messagesInfo.MessagesId);

            if(!messagesToDelete.All(x => x.ConversationID == messagesInfo.ConversationId))
            {
                throw new ArgumentException("All messages must be from same conversation passed as ConversationId parameter.");
            }

            var conversation = await usersConversationsRepository.Get(whoAccessedId, messagesInfo.ConversationId);

            if(conversation == null)
            {
                throw unAuthorizedError;
            }

            await messagesRepository.Remove(messagesInfo.MessagesId, whoAccessedId);
        } 

        #endregion
    }
}
