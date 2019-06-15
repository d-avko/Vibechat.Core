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
using Vibechat.Web.Data.DataModels;
using Vibechat.Web.Extensions;
using Vibechat.Web.Services.ChatDataProviders;
using Vibechat.Web.Services.Crypto;
using Vibechat.Web.Services.Extension_methods;
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
            IDhPublicKeysRepository dh,
            ImagesService imagesService,
            CryptoService cryptoService)
        {
            this.chatDataProvider = chatDataProvider;
            this.usersRepository = usersRepository;
            this.messagesRepository = messagesRepository;
            this.attachmentRepository = attachmentRepository;
            this.attachmentKindsRepository = attachmentKindsRepository;
            this.usersConversationsRepository = usersConversationsRepository;
            this.conversationRepository = conversationRepository;
            this.dh = dh;
            ImagesService = imagesService;
            this.cryptoService = cryptoService;
        }

        protected readonly IChatDataProvider chatDataProvider;

        protected readonly IUsersRepository usersRepository;

        protected readonly IMessagesRepository messagesRepository;

        protected readonly IAttachmentRepository attachmentRepository;

        protected readonly IAttachmentKindsRepository attachmentKindsRepository;

        protected readonly IUsersConversationsRepository usersConversationsRepository;

        protected readonly IConversationRepository conversationRepository;

        private readonly IDhPublicKeysRepository dh;

        private const int MaxThumbnailLengthMB = 5;

        private const int MaxNameLength = 200;

        protected readonly ImagesService ImagesService;
        private readonly CryptoService cryptoService;

        #region Conversations

        public async Task UpdateAuthKey(int chatId, string authKeyId, string thisUserId)
        {
            if(!await ExistsInConversation(chatId, thisUserId))
            {
                throw new InvalidDataException($"Wrong conversation id was provided.");
            }

            ConversationDataModel chat = conversationRepository.GetById(chatId);

            if (chat == null)
            {
                throw new InvalidDataException($"Wrong conversation id was provided.");
            }

            await conversationRepository.UpdateAuthKey(chat, authKeyId);
        }

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

                //key to secured conversation could be lost.
                if(!convInfo.IsSecure && await usersConversationsRepository.DialogExists(user.Id, SecondDialogueUser.Id))
                {
                    throw new FormatException("Dialog already exists.");
                }
                var imageUrl = convInfo.ImageUrl ?? chatDataProvider.GetGroupPictureUrl();

                ConversationToAdd = new ConversationDataModel()
                {
                    IsGroup = convInfo.IsGroup,
                    Name = convInfo.IsGroup ? convInfo.ConversationName : null,
                    FullImageUrl = imageUrl,
                    ThumbnailUrl = imageUrl,
                    Creator = user,
                    IsPublic = convInfo.IsPublic,
                    IsSecure = convInfo.IsSecure
                };

               
                await conversationRepository.Add(ConversationToAdd);

                if (convInfo.IsSecure)
                {
                    var generated = cryptoService.GenerateDhPublicKey();
                    var key = new DhPublicKeyDataModel()
                    {
                        Generator = generated.Generator,
                        Modulus = generated.Modulus
                    };

                    await dh.Add(key);

                    await conversationRepository.SetPublicKey(ConversationToAdd, key);
                }

                await usersConversationsRepository.Add(SecondDialogueUser, ConversationToAdd);
            }
            else
            {
                
                var imageUrl = convInfo.ImageUrl ?? chatDataProvider.GetProfilePictureUrl();

                ConversationToAdd = new ConversationDataModel()
                {
                    IsGroup = convInfo.IsGroup,
                    Name = convInfo.IsGroup ? convInfo.ConversationName : null,
                    FullImageUrl = imageUrl,
                    ThumbnailUrl = imageUrl,
                    Creator = user,
                    IsPublic = convInfo.IsPublic,
                    IsSecure = convInfo.IsSecure
                };

                await conversationRepository.Add(ConversationToAdd);
            }

            await usersConversationsRepository.Add(user, ConversationToAdd);

            //after saving changes we have Id of our 
            //created conversation in ConversationToAdd variable
            return ConversationToAdd.ToConversationTemplate(
                await GetParticipants(new GetParticipantsApiModel() { ConvId = ConversationToAdd.ConvID }),
                null,
                SecondDialogueUser
                );
        }

        public async Task<int> GetUnreadMessagesAmount(int conversationId, string userId)
        {
            ConversationDataModel conversation = conversationRepository.GetById(conversationId);

            if (conversation == null)
            {
                throw new InvalidDataException($"Wrong conversation id was provided.");
            }

            if (messagesRepository.Empty())
            {
                return 0;
            }

            return messagesRepository.GetUnreadAmount(conversationId, userId);
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

        public async Task<List<Message>> GetAttachments(string kind, int conversationId, string whoAccessedId, int offset, int count)
        {
            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            if (messagesRepository.Empty())
            {
                return null;
            }

            var conversation = conversationRepository.GetById(conversationId);

            if (conversation == null)
            {
                throw new FormatException("Wrong conversation to get attachments from.");
            }

            var members = usersConversationsRepository.GetConversationParticipants(conversationId);

            //only member of conversation could request messages of non-public conversation.

            if (members.FirstOrDefault(x => x.Id == whoAccessedId) == null && !conversation.IsPublic)
            {
                throw new UnauthorizedAccessException("You are unauthorized to do such an action.");
            }

            var messages = messagesRepository.GetAttachments(
                whoAccessedId,
                conversationId,
                kind,
                offset,
                count)
                .Include(x => x.AttachmentInfo)
                .ThenInclude(x => x.AttachmentKind)
                .Include(x => x.User)
                .Include(x => x.ForwardedMessage)
                .ThenInclude(x => x.AttachmentInfo)
                .ThenInclude(x => x.AttachmentKind)
                .Include(x => x.ForwardedMessage)
                .ThenInclude(x => x.User);

            return (from msg in messages
                    select msg.ToMessage()).ToList();

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
                        await GetParticipants(new GetParticipantsApiModel() { ConvId = conversation.ConvID }),
                        await GetMessages(new GetMessagesApiModel() { ConversationID = conversation.ConvID, Count = 1, MesssagesOffset = 0 }, whoAccessedId),
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


        public async Task RemoveUserFromConversation(string userId, string whoRemovedId, int conversationId)
        {
            var conversation = conversationRepository.GetById(conversationId);

            if (conversation == null)
            {
                throw new FormatException("Wrong conversation.");
            }

            var userConversation = await usersConversationsRepository.Get(userId, conversationId);

            if (userConversation == null)
            {
                throw new FormatException("User is not a part of this conversation.");
            }

            if(conversation.Creator.Id != whoRemovedId && !conversation.IsGroup)
            {
                throw new FormatException("Only creator can remove users in group.");
            }

            await usersConversationsRepository.Remove(userConversation);

            //if last user LEAVES the group, remove conversation

            if(usersConversationsRepository.GetConversationParticipants(conversationId).Count().Equals(0))
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
                    await RemoveUserFromConversation(user.Id, whoRemoves, conversation.ConvID);
                }


                conversationRepository.Remove(conversation);
            }
            else
            {
                await RemoveUserFromConversation(Conversation.DialogueUser.Id, whoRemoves, conversation.ConvID);
                await RemoveUserFromConversation(whoRemoves, whoRemoves, conversation.ConvID);

                var messages = messagesRepository.Get(whoRemoves, conversation.ConvID, true);

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

        public async Task ValidateDialog(string firstUserId, string secondUserId, int conversationId)
        {
            var dialog = await usersConversationsRepository.GetDialog(firstUserId, secondUserId);

            if(dialog == null)
            {
                throw new InvalidDataException("Wrong id of a user in dialog.");
            }
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
                         await GetParticipants(new GetParticipantsApiModel() { ConvId = conversation.ConvID }),
                         //only get last message here, client should fetch messages after he opened the conversation.
                         await GetMessages(new GetMessagesApiModel() { ConversationID = conversation.ConvID, Count = 1, MesssagesOffset = 0 }, whoAccessedId),
                         DialogUser)
                    );
            }

            return returnData;

        }


        public async Task<List<UserInfo>> GetParticipants(GetParticipantsApiModel convInfo)
        {
            var defaultErrorMessage = new FormatException("Wrong conversation was provided.");

            if (convInfo == null)
                throw defaultErrorMessage;

            var conversation = conversationRepository.GetById(convInfo.ConvId);

            if (conversation == null)
                throw defaultErrorMessage;

            var participants = usersConversationsRepository.GetConversationParticipants(convInfo.ConvId);

            return (from participant in participants
                    select participant.ToUserInfo()
                               ).ToList();

        }


        public async Task<List<Message>> GetMessages(GetMessagesApiModel convInfo, string whoAccessedId)
        {
            var defaultErrorMessage = new FormatException("Wrong conversation was provided.");

            var unAuthorizedError = new UnauthorizedAccessException("You are unauthorized to do such an action.");

            if (messagesRepository.Empty())
            {
                return null;
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

            var messages = messagesRepository.Get(
                whoAccessedId,
                convInfo.ConversationID,
                false,
                convInfo.MesssagesOffset,
                convInfo.Count)
                .Include(x => x.AttachmentInfo)
                .ThenInclude(x => x.AttachmentKind)
                .Include(x => x.User)
                .Include(x => x.ForwardedMessage)
                .ThenInclude(x => x.AttachmentInfo)
                .ThenInclude(x => x.AttachmentKind)
                .Include(x => x.ForwardedMessage)
                .ThenInclude(x => x.User);

            return (from msg in messages
                    select msg.ToMessage()).ToList();

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

        public async Task<ConversationTemplate> GetByIdSimplified(int conversationId, string whoAccessedId)
        {
            ConversationDataModel conversation = conversationRepository.GetById(conversationId);

            if (conversation == null)
            {
                throw new FormatException("No such conversation was found.");
            }

            var dialogUser = new UserInApplication();

            if (!conversation.IsGroup)
            {
                dialogUser = usersConversationsRepository.GetUserInDialog(conversationId, whoAccessedId);

                if (dialogUser == null)
                {
                    throw new FormatException("Unexpected error: no corresponding user in dialogue.");
                }
            }

            return new ConversationTemplate()
            {
                DialogueUser = dialogUser?.ToUserInfo(),
                IsGroup = conversation.IsGroup
            }; 
        }

        public async Task MarkMessageAsRead(int msgId, int conversationId, string whoAccessedId)
        {
            MessageDataModel message = messagesRepository.GetById(msgId);

            if (message.User.Id == whoAccessedId)
            {
                throw new UnauthorizedAccessException("Couldn't mark this message as read because it was sent by you.");
            }

            if(!await ExistsInConversation(conversationId, whoAccessedId))
            {
                throw new UnauthorizedAccessException("User was not present in conversation. Couldn't mark the message as read.");
            }

            messagesRepository.MarkAsRead(message);
        }

        public async Task<MessageDataModel> AddMessage(Message message, int groupId, string SenderId)
        {
            UserInApplication whoSent = await usersRepository.GetById(SenderId);

            if (whoSent == null)
            {
                throw new FormatException($"Failed to retrieve user with id {SenderId} from database: no such user exists");
            }

            MessageDataModel forwardedMessage = null;

            if(message.ForwardedMessage != null)
            {
                var foundMessage = messagesRepository.GetByIds(new List<int>() { message.ForwardedMessage.Id }).ToList();

                if (!foundMessage.Count().Equals(1))
                {
                    throw new FormatException($"Forwarded message was not found.");
                }

                forwardedMessage = foundMessage[0];
            }

            return await messagesRepository.Add(whoSent, message, groupId, forwardedMessage);
        }

        public async Task<MessageDataModel> AddEncryptedMessage(string message, int groupId, string SenderId)
        {
            UserInApplication whoSent = await usersRepository.GetById(SenderId);

            if (whoSent == null)
            {
                throw new FormatException($"Failed to retrieve user with id {SenderId} from database: no such user exists");
            }

            return await messagesRepository.AddSecureMessage(whoSent, message, groupId);
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

            IQueryable<MessageDataModel> messagesToDelete = messagesRepository.GetByIds(messagesInfo.MessagesId);

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
