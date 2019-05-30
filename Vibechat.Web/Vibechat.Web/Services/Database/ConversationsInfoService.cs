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
using Vibechat.Web.Extensions;
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

            if (string.IsNullOrWhiteSpace(convInfo.CreatorId))
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
            ConversationDataModel ConversationToAdd;

            if (!convInfo.IsGroup)
            {
                //if this is a dialogue , find a user with whom to create conversation
                SecondDialogueUser = await usersRepository.GetById(convInfo.DialogUserId);

                if (SecondDialogueUser == null)
                {
                    throw defaultError;
                }

                if(await usersConversationsRepository.DialogExists(user.Id, SecondDialogueUser.Id))
                {
                    throw new FormatException("Dialog already exists.");
                }

                ConversationToAdd = await conversationRepository.Add(
                    convInfo.IsGroup,
                    convInfo.IsGroup ? convInfo.ConversationName : null,
                    convInfo.ImageUrl ?? chatDataProvider.GetProfilePictureUrl(),
                    user,
                    convInfo.IsPublic
                    );


                await usersConversationsRepository.Add(SecondDialogueUser, ConversationToAdd);
            }
            else
            {
                ConversationToAdd = await conversationRepository.Add(
                    convInfo.IsGroup,
                    convInfo.IsGroup ? convInfo.ConversationName : null,
                    convInfo.ImageUrl ?? chatDataProvider.GetGroupPictureUrl(),
                    user,
                    convInfo.IsPublic
                    );
            }

            await usersConversationsRepository.Add(user, ConversationToAdd);

            //after saving changes we have Id of our 
            //created conversation in ConversationToAdd variable
            return ConversationToAdd.ToConversationTemplate(
                (await GetParticipants(new GetParticipantsApiModel() { ConvId = ConversationToAdd.ConvID })).Participants,
                null,
                SecondDialogueUser
                );
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
                var thumbnailFull = ImagesService.SaveImage(buffer, image.FileName);

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

            var whoSearches = await usersRepository.GetById(whoAccessedId);

            var groups = await conversationRepository.SearchByName(name, whoSearches, usersConversationsRepository);

            if(groups == null)
            {
                return null;
            }

            var result = new List<ConversationTemplate>();

            foreach (var conversation in groups)
            {
                result.Add(
                    conversation.ToConversationTemplate(
                        (await GetParticipants(new GetParticipantsApiModel() { ConvId = conversation.ConvID })).Participants,
                        (await GetMessages(new GetMessagesApiModel() { ConversationID = conversation.ConvID, Count = 1, MesssagesOffset = 0 }, whoAccessedId)).Messages,
                        null
                    ));
            }

            return result;
        }

        public async Task ChangePublicState(int conversationId, string whoAccessedId)
        {
            var conversation = conversationRepository.GetById(conversationId);

            if(conversation.Creator.Id != whoAccessedId)
            {
                throw new FormatException("This method is only allowed to group creator.");
            }

            await conversationRepository.ChangePublicState(conversationId);
        }


        public async Task RemoveUserFromConversation(string userId, string whoRemovedId, int conversationId, bool IsSelf)
        {
            var conversation = conversationRepository.GetById(conversationId);

            if (conversation == null)
            {
                throw new FormatException("Wrong conversation.");
            }

            if (!IsSelf && userId == whoRemovedId && conversation.IsGroup)
            {
                throw new FormatException("Couldn't remove yourself from group.");
            }

            var userConversation = await usersConversationsRepository.Get(userId, conversationId);

            if (userConversation == null)
            {
                throw new FormatException("User is not a part of this conversation.");
            }

            if(conversation.Creator.Id != whoRemovedId && !IsSelf && !conversation.IsGroup)
            {
                throw new FormatException("Only creator can remove users in group.");
            }

            await usersConversationsRepository.Remove(userConversation);

            //if last user LEAVES the group, remove conversation

            if(IsSelf && usersConversationsRepository.GetConversationParticipants(conversationId).Count().Equals(0))
            {
                conversationRepository.Remove(conversation);
            }
        }

        public async Task RemoveConversation(ConversationTemplate Conversation, string whoRemoves)
        {
            ConversationDataModel conversation = conversationRepository.GetById(Conversation.ConversationID);

            if(conversation == null)
            {
                throw new FormatException("Wrong conversation to remove.");
            }

            if (conversation.Creator.Id != whoRemoves && conversation.IsGroup)
            {
                throw new FormatException("Only creator can remove group.");
            }

            if (conversation.IsGroup)
            {
                foreach(var user in Conversation.Participants)
                {
                    await RemoveUserFromConversation(user.Id, whoRemoves, conversation.ConvID, false);
                }


                conversationRepository.Remove(conversation);
            }
            else
            {
                await RemoveUserFromConversation(Conversation.DialogueUser.Id, whoRemoves, conversation.ConvID, false);
                await RemoveUserFromConversation(whoRemoves, whoRemoves, conversation.ConvID, false);

                var messages = messagesRepository.GetMessagesForConversation(whoRemoves, conversation.ConvID, true);

                await attachmentRepository.Remove(
                    messages
                    .Where(x => x.IsAttachment)
                    .Select(x => x.AttachmentInfo)
                    .ToList());

                conversationRepository.Remove(conversation);
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

            return addedUser.ToUserInfo();
        }

        public async Task<List<ConversationTemplate>> GetConversations(string whoAccessedId)
        {
            var defaultError = new FormatException("User info provided was not correct.");

            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            if (whoAccessedId == null)
                throw defaultError;

            var user = await usersRepository.GetById(whoAccessedId).ConfigureAwait(false);

            if (user == null)
                throw defaultError;


            IQueryable<ConversationDataModel> conversations = usersConversationsRepository.GetUserConversations(whoAccessedId);

            var returnData = new List<ConversationTemplate>();

            foreach (ConversationDataModel conversation in conversations)
            {
                UserInApplication DialogUser = conversation.IsGroup ? null : usersConversationsRepository.GetUserInDialog(conversation.ConvID, whoAccessedId);

                returnData.Add
                    (
                    conversation.ToConversationTemplate(
                         (await GetParticipants(new GetParticipantsApiModel() { ConvId = conversation.ConvID })).Participants,
                         //only get last message here, client should fetch messages after he opened the conversation.
                         (await GetMessages(new GetMessagesApiModel() { ConversationID = conversation.ConvID, Count = 1, MesssagesOffset = 0 }, whoAccessedId)).Messages,
                         DialogUser)
                    );
            }

            return returnData;

        }


        public async Task<GetParticipantsResultApiModel> GetParticipants(GetParticipantsApiModel convInfo)
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
                               select participant.ToUserInfo()
                               ).ToList()
             };

        }


        public async Task<GetMessagesResultApiModel> GetMessages(GetMessagesApiModel convInfo, string whoAccessedId)
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
                false,
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

        public async Task<bool> ExistsInConversation(int conversationsId, string userId)
        {
            return await usersConversationsRepository.Exists(userId, conversationsId);
        }

        public async Task<ConversationTemplate> GetById(int conversationId, string whoAccessedId)
        {
             
            ConversationDataModel conversation = conversationRepository.GetById(conversationId);

            if (conversation == null)
            {
                throw new FormatException("No such conversation was found.");
            }

            var dialogUser = new UserInApplication();
            var members = new List<UserInfo>();
            var messages = new List<Message>();

            if (conversation.IsGroup)
            {
                members = usersConversationsRepository.GetConversationParticipants(conversationId)
                .Select(x => x.ToUserInfo())
                .ToList();
            }
            else
            {
                dialogUser = usersConversationsRepository.GetUserInDialog(conversationId,whoAccessedId);

                if (dialogUser == null)
                {
                    throw new FormatException("Unexpected error: no corresponding user in dialogue.");
                }
            }

            return conversation.ToConversationTemplate(members, null, dialogUser);

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
