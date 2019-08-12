using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VibeChat.Web;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;
using Vibechat.Web.Data.ApiModels.Conversation;
using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Data.DataModels;
using Vibechat.Web.Data.Repositories;
using Vibechat.Web.Data_Layer.Repositories;
using Vibechat.Web.Extensions;
using Vibechat.Web.Services.Bans;
using Vibechat.Web.Services.ChatDataProviders;
using Vibechat.Web.Services.FileSystem;
using Vibechat.Web.Services.Messages;

namespace Vibechat.Web.Services
{
    public class ChatService
    {
        private const int MaxThumbnailLengthMb = 5;

        private const int MaxNameLength = 200;

        private const int MaxParticipantsToReturn = 200;

        private readonly BansService bansService;
        
        private readonly MessagesService messagesService;

        private readonly IComparer<Chat> chatComparer;

        private readonly IChatDataProvider chatDataProvider;

        private readonly IConversationRepository conversationRepository;

        private readonly FilesService imagesService;

        private readonly ILastMessagesRepository lastMessagesRepository;

        private readonly IMessagesRepository messagesRepository;

        private readonly IDhPublicKeysRepository publicKeys;

        private readonly IChatRolesRepository rolesRepository;

        private readonly UnitOfWork unitOfWork;

        private readonly IUsersConversationsRepository usersConversationsRepository;

        private readonly IUsersRepository usersRepository;

        public ChatService(
            IChatDataProvider chatDataProvider,
            IUsersRepository usersRepository,
            IMessagesRepository messagesRepository,
            IUsersConversationsRepository usersConversationsRepository,
            IConversationRepository conversationRepository,
            ILastMessagesRepository lastMessagesRepository,
            IDhPublicKeysRepository dh,
            IChatRolesRepository rolesRepository,
            FilesService imagesService,
            UnitOfWork unitOfWork,
            BansService bansService,
            MessagesService messagesService,
            IComparer<Chat> chatComparer)
        { 
            this.chatDataProvider = chatDataProvider;
            this.usersRepository = usersRepository;
            this.messagesRepository = messagesRepository;
            this.usersConversationsRepository = usersConversationsRepository;
            this.conversationRepository = conversationRepository;
            this.lastMessagesRepository = lastMessagesRepository;
            publicKeys = dh;
            this.rolesRepository = rolesRepository;
            this.imagesService = imagesService;
            this.unitOfWork = unitOfWork;
            this.bansService = bansService;
            this.messagesService = messagesService;
            this.chatComparer = chatComparer;
        }

        public async Task UpdateAuthKey(int chatId, string authKeyId, string deviceId, string thisUserId)
        {
            if (!await ExistsInConversation(chatId, thisUserId))
            {
                throw new InvalidDataException("Wrong conversation id was provided.");
            }

            var chat = conversationRepository.GetById(chatId);

            if (chat == null)
            {
                throw new InvalidDataException("Wrong conversation id was provided.");
            }

            conversationRepository.UpdateAuthKey(chat, authKeyId);

            usersConversationsRepository.UpdateDeviceId(deviceId, thisUserId, chatId);

            await unitOfWork.Commit();
        }

        public async Task<Chat> CreateConversation(CreateConversationCredentialsApiModel credentials)
        {
            var defaultError = new FormatException("Error while creating the conversation..");

            //if there was no group info or creator info

            if (string.IsNullOrWhiteSpace(credentials.CreatorId))
            {
                throw defaultError;
            }

            var user = await usersRepository.GetById(credentials.CreatorId);

            if (user == null)
            {
                throw defaultError;
            }

            if (!credentials.IsGroup && credentials.DialogUserId == null)
            {
                throw new FormatException("No dialogue user was provided...");
            }

            if (credentials.IsGroup && credentials.ConversationName.Length > MaxNameLength)
            {
                throw new FormatException("Chat name was too long...");
            }

            AppUser secondDialogueUser = null;
            DhPublicKeyDataModel dhPublicKey = null;

            var imageUrl = string.Empty;

            if (!credentials.IsGroup)
            {
                //if this is a dialogue , find a user with whom to create conversation
                secondDialogueUser = await usersRepository.GetById(credentials.DialogUserId);

                if (secondDialogueUser == null)
                {
                    throw defaultError;
                }

                if (credentials.IsSecure)
                {
                    dhPublicKey = await publicKeys.GetRandomKey();
                }
            }
            else
            {
                imageUrl = credentials.ImageUrl ?? chatDataProvider.GetGroupPictureUrl();
            }

            var createdChat = new ConversationDataModel
            {
                IsGroup = credentials.IsGroup,
                Name = credentials.IsGroup ? credentials.ConversationName : null,
                FullImageUrl = imageUrl,
                ThumbnailUrl = imageUrl,
                IsPublic = credentials.IsPublic,
                IsSecure = credentials.IsSecure,
                PublicKeyId = dhPublicKey?.Id,
                PublicKey = dhPublicKey
            };

            conversationRepository.Add(createdChat);

            try
            {
                if (!credentials.IsGroup)
                {
                    usersConversationsRepository.Add(credentials.DialogUserId, createdChat.Id);
                    rolesRepository.Add(createdChat.Id, credentials.DialogUserId, ChatRole.NoRole);
                }

                usersConversationsRepository.Add(credentials.CreatorId, createdChat.Id, credentials.DeviceId);

                rolesRepository.Add(createdChat.Id, credentials.CreatorId, ChatRole.Creator);

                await unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Couldn't create group/dialog. Probably there exists one already.", ex);
            }

            var creator = user.ToUserInfo();
            creator.ChatRole = new ChatRoleDto
            {
                ChatId = createdChat.Id,
                Role = ChatRole.Creator
            };

            return createdChat.ToChatDto(
                credentials.IsGroup ? new List<UserInfo> {creator} : null,
                secondDialogueUser,
                dhPublicKey,
                await rolesRepository.GetAsync(createdChat.Id, credentials.CreatorId),
                credentials.DeviceId,
                0,
                null
            );
        }

        public async Task<List<UserInfo>> FindUsersInChat(int chatId, string username, string caller)
        {
            if (!await usersConversationsRepository.Exists(caller, chatId))
            {
                throw new UnauthorizedAccessException("Caller of this method must be the part of conversation.");
            }

            var result = (await usersConversationsRepository.FindUsersInChat(username, chatId))
                .Select(x => x.ToUserInfo())
                .ToList();
            
            foreach (var user in result)
            {
                user.IsBlockedInConversation =
                    await bansService.IsBannedFromConversation(chatId, user.Id);
                user.ChatRole = await GetChatRole(user.Id, chatId);
            }

            return result;
        }

        private async Task<int> GetUnreadMessagesAmount(Chat chat, string userId)
        {
            if (messagesRepository.Empty())
            {
                return 0;
            }

            var lastMessage = lastMessagesRepository.Get(userId, chat.Id);

            return messagesRepository.GetUnreadAmount(chat.Id, userId, lastMessage?.MessageID ?? 0);
        }

        public async Task<UpdateThumbnailResponse> UpdateThumbnail(int conversationId, IFormFile image, string userId)
        {
            if (image.Length / (1024 * 1024) > MaxThumbnailLengthMb)
            {
                throw new InvalidDataException($"Thumbnail was larger than {MaxThumbnailLengthMb}");
            }

            var conversation = conversationRepository.GetById(conversationId);

            if (conversation == null)
            {
                throw new InvalidDataException("Wrong conversation id was provided.");
            }

            try
            {
                var (thumbnail, fullsized) = await imagesService.SaveProfileOrChatPicture(image, image.FileName,
                    conversationId.ToString(), userId);
 
                conversationRepository.UpdateThumbnail(thumbnail, fullsized, conversation);
                await unitOfWork.Commit();

                return new UpdateThumbnailResponse
                {
                    ThumbnailUrl = conversation.ThumbnailUrl,
                    FullImageUrl = conversation.FullImageUrl
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update chat image. Try different one.", ex);
            }
        }

        public async Task ChangeName(int conversationId, string name)
        {
            if (name.Length > MaxNameLength)
            {
                throw new InvalidDataException($"Name length couldn't be more than {MaxNameLength}.");
            }

            var conversation = conversationRepository.GetById(conversationId);

            if (conversation == null)
            {
                throw new InvalidDataException("Wrong conversation id was provided.");
            }

            conversationRepository.ChangeName(conversation, name);
            await unitOfWork.Commit();
        }

        public async Task<List<Chat>> SearchForGroups(string name, string whoAccessedId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidDataException("Search string coudn't be null.");
            }

            var groups = conversationRepository.SearchByName(name);

            if (groups == null)
            {
                return null;
            }

            var result = new List<Chat>();

            foreach (var conversation in groups)
            {
                result.Add(
                    conversation.ToChatDto(
                        await GetParticipants(conversation.Id, MaxParticipantsToReturn),
                        null,
                        conversation.PublicKey,
                        await rolesRepository.GetAsync(conversation.Id, whoAccessedId),
                        null,
                        0,
                        null
                    ));
            }

            return result;
        }

        public async Task ChangePublicState(int conversationId, string whoAccessedId)
        {
            var conversation = conversationRepository.GetById(conversationId);
            var userRole = await rolesRepository.GetAsync(conversationId, whoAccessedId);

            if (userRole.RoleId != ChatRole.Moderator && userRole.RoleId != ChatRole.Creator)
            {
                throw new FormatException("This method is only allowed to group creator / moderator.");
            }

            conversationRepository.ChangePublicState(conversation);
            await unitOfWork.Commit();
        }

        /// <summary>
        ///     Should not be used frequently as it's quiet expensive.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="whoRemovedId"></param>
        /// <param name="chatId"></param>
        /// <param name="IsSelf"></param>
        /// <returns></returns>
        public async Task RemoveUserFromConversation(string userId, string whoRemovedId, int chatId, bool IsSelf)
        {
            var conversation = conversationRepository.GetById(chatId);

            if (conversation == null)
            {
                throw new FormatException("Wrong conversation.");
            }

            var userConversation = await usersConversationsRepository.Get(userId, chatId);

            if (userConversation == null)
            {
                throw new FormatException("User is not a part of this conversation.");
            }

            var userRole = await rolesRepository.GetAsync(chatId, whoRemovedId);

            if (!IsSelf)
            {
                if (userRole.RoleId != ChatRole.Moderator && userRole.RoleId != ChatRole.Creator)
                {
                    throw new FormatException("Only creator / moderator can remove users in group.");
                }
            }

            usersConversationsRepository.Remove(userConversation);

            if (usersConversationsRepository.GetParticipantsCount(conversation.Id).Equals(0))
            {
                conversationRepository.Remove(conversation);
            }

            rolesRepository.Remove(await rolesRepository.GetAsync(chatId, userId));

            await unitOfWork.Commit();
        }

        public async Task RemoveConversation(Chat Conversation, string whoRemoves)
        {
            var conversation = conversationRepository.GetById(Conversation.Id);

            if (conversation == null)
            {
                throw new FormatException("Wrong conversation to remove.");
            }

            //secure chats may be removed by any of participants.

            var userRole = await rolesRepository.GetAsync(conversation.Id, whoRemoves);

            if (!conversation.IsSecure)
            {
                if (conversation.IsGroup)
                {
                    if (userRole.RoleId != ChatRole.Creator)
                    {
                        throw new FormatException("Only creator can remove group.");
                    }
                }
            }

            //this removes all messages and users-chats links
            conversationRepository.Remove(conversation);

            await unitOfWork.Commit();
        }

        public async Task ChangeUserRole(int chatId, string userId, string caller, ChatRole newRole)
        {
            var callerRole = await rolesRepository.GetAsync(chatId, caller);

            if (callerRole == null)
            {
                throw new UnauthorizedAccessException("Only creator can change user's roles.");
            }

            if (callerRole.RoleId != ChatRole.Creator)
            {
                throw new UnauthorizedAccessException("Only creator can change user's roles.");
            }

            if (userId == caller)
            {
                throw new UnauthorizedAccessException("Can't change role of creator.");
            }

            var targetUserRole = await rolesRepository.GetAsync(chatId, userId);

            rolesRepository.Update(newRole, targetUserRole);

            await unitOfWork.Commit();
        }

        public async Task<UserInfo> AddUserToConversation(int chatId, string userId)
        {
            var defaultError = new FormatException("Invalid credentials were provided.");

            var FoundConversation = conversationRepository.GetById(chatId);

            if (FoundConversation == null)
            {
                throw defaultError;
            }

            var FoundUser = await usersRepository.GetById(userId);

            if (FoundUser == null)
            {
                throw defaultError;
            }

            if (await usersConversationsRepository.Exists(FoundUser.Id, FoundConversation.Id))
            {
                throw new FormatException("User already exists in converation.");
            }

            var addedUser = usersConversationsRepository.Add(userId, chatId).User;
            rolesRepository.Add(chatId, userId, ChatRole.NoRole);

            await unitOfWork.Commit();
            return addedUser.ToUserInfo();
        }

        public async Task ValidateDialog(string firstUserId, string secondUserId, int conversationId)
        {
            var dialog = await usersConversationsRepository.GetDialog(firstUserId, secondUserId);

            if (dialog == null)
            {
                throw new InvalidDataException("Wrong id of a user in dialog.");
            }
        }

        public async Task<List<ChatRoleDto>> GetChatRoles(string userId)
        {
            var roles = await rolesRepository.GetAsync(userId);

            return (from role in roles
                select role.ToChatRole()).ToList();
        }

        public async Task<ChatRoleDto> GetChatRole(string userid, int chatId)
        {
            return (await rolesRepository.GetAsync(chatId, userid)).ToChatRole();
        }
        
        /// <summary>
        /// Returns chats sorted by most recent last message.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="whoAccessedId"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public async Task<List<Chat>> GetConversations(string deviceId, string whoAccessedId)
        {
            var defaultError = new FormatException("User info provided was not correct.");

            if (whoAccessedId == null)
            {
                throw defaultError;
            }

            var user = await usersRepository.GetById(whoAccessedId).ConfigureAwait(false);

            if (user == null)
            {
                throw defaultError;
            }

            var chats = usersConversationsRepository.GetUserConversations(deviceId, whoAccessedId);

            var returnData = new List<Chat>();

            foreach (var chat in chats)
            {
                var lastMessage = (await messagesService.GetMessages(
                        chat.Id, 
                        0, 
                        1, 
                        -1, 
                        false, 
                        false, 
                        whoAccessedId))
                    .FirstOrDefault();

                var dtoChat = chat.ToChatDto(
                    chat.IsGroup ? await GetParticipants(chat.Id, MaxParticipantsToReturn) : null,
                    chat.IsGroup
                        ? null
                        : usersConversationsRepository.GetUserInDialog(chat.Id, whoAccessedId),
                    chat.PublicKey,
                    await rolesRepository.GetAsync(chat.Id, whoAccessedId),
                    chat.IsSecure ? await GetDeviceId(chat.Id, whoAccessedId) : null,
                    lastMessagesRepository.Get(whoAccessedId, chat.Id)?.MessageID ?? 0,
                    lastMessage
                );

                dtoChat.IsMessagingRestricted =
                    await bansService.IsBannedFromConversation(chat.Id, whoAccessedId);
                dtoChat.MessagesUnread = await GetUnreadMessagesAmount(dtoChat, whoAccessedId);
                dtoChat.ChatRole = await GetChatRole(whoAccessedId, chat.Id);

                if (chat.IsGroup)
                {
                    foreach (var User in dtoChat.Participants)
                    {
                        User.IsBlockedInConversation =
                            await bansService.IsBannedFromConversation(chat.Id, User.Id);
                        User.ChatRole = await GetChatRole(User.Id, chat.Id);
                    }
                }

                returnData.Add(dtoChat);
            }

            returnData.Sort(chatComparer);
            
            return returnData;
        }

        private async Task<string> GetDeviceId(int chatId, string userId)
        {
            var chat = await usersConversationsRepository.Get(userId, chatId);
            return chat.DeviceId;
        }

        public async Task<List<UserInfo>> GetParticipants(int chatId, int maximum = 0)
        {
            var defaultErrorMessage = new FormatException("Wrong conversation was provided.");

            var conversation = conversationRepository.GetById(chatId);

            if (conversation == null)
            {
                throw defaultErrorMessage;
            }

            var participants = usersConversationsRepository.GetConversationParticipants(chatId);

            var users = (from participant in participants
                    select participant.ToUserInfo()
                ).ToList();

            if (maximum.Equals(0))
            {
                return users;
            }

            return users.Take(maximum).ToList();
        }
        

        public async Task<bool> ExistsInConversation(int conversationsId, string userId)
        {
            return await usersConversationsRepository.Exists(userId, conversationsId);
        }

        /// <summary>
        ///     Gets specified chat info by id. Doesn't support secure chats.
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="whoAccessedId"></param>
        /// <returns></returns>
        public async Task<Chat> GetById(int conversationId, string whoAccessedId)
        {
            var conversation = conversationRepository.GetById(conversationId);

            if (conversation == null)
            {
                throw new FormatException("No such conversation was found.");
            }

            var dialogUser = new AppUser();
            var members = new List<UserInfo>();

            if (conversation.IsGroup)
            {
                members = usersConversationsRepository.GetConversationParticipants(conversationId)
                    .Select(x => x.ToUserInfo())
                    .ToList();

                if (!conversation.IsPublic && !members.Any(usr => usr.Id == whoAccessedId))
                {
                    throw new UnauthorizedAccessException("You are not allowed to view this chat.");
                }
            }
            else
            {
                dialogUser = usersConversationsRepository.GetUserInDialog(conversationId, whoAccessedId);

                if (dialogUser == null)
                {
                    throw new FormatException("Unexpected error: no corresponding user in dialogue.");
                }
            }
            

            var dtoChat = conversation.ToChatDto(
                members,
                dialogUser,
                conversation.PublicKey,
                await rolesRepository.GetAsync(conversation.Id, whoAccessedId),
                null,
                0,
                null);

            dtoChat.IsMessagingRestricted = await bansService.IsBannedFromConversation(dtoChat.Id, whoAccessedId);
            dtoChat.ClientLastMessageId = lastMessagesRepository.Get(whoAccessedId, conversationId)?.MessageID ?? 0;
            
            if (conversation.IsGroup)
            {
                foreach (var User in dtoChat.Participants)
                {
                    User.IsBlockedInConversation = await bansService.IsBannedFromConversation(conversation.Id, User.Id);
                    User.ChatRole = await GetChatRole(User.Id, conversation.Id);
                }
            }

            return dtoChat;
        }

        public async Task<Chat> GetByIdSimplified(int conversationId, string whoAccessedId)
        {
            var conversation = conversationRepository.GetById(conversationId);

            if (conversation == null)
            {
                throw new FormatException("No such conversation was found.");
            }

            var dialogUser = new AppUser();

            if (!conversation.IsGroup)
            {
                dialogUser = usersConversationsRepository.GetUserInDialog(conversationId, whoAccessedId);

                if (dialogUser == null)
                {
                    throw new FormatException("Unexpected error: no corresponding user in dialogue.");
                }
            }

            return new Chat
            {
                DialogueUser = dialogUser?.ToUserInfo(),
                IsGroup = conversation.IsGroup
            };
        }
    }
}