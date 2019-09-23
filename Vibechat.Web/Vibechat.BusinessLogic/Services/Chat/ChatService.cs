using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vibechat.BusinessLogic.Extensions;
using Vibechat.BusinessLogic.Services.Bans;
using Vibechat.BusinessLogic.Services.ChatDataProviders;
using Vibechat.BusinessLogic.Services.FileSystem;
using Vibechat.BusinessLogic.Services.Messages;
using Vibechat.DataLayer;
using Vibechat.DataLayer.DataModels;
using Vibechat.DataLayer.Repositories;
using Vibechat.DataLayer.Repositories.Specifications.Chats;
using Vibechat.DataLayer.Repositories.Specifications.UsersChats;
using Vibechat.Shared.ApiModels.Conversation;
using Vibechat.Shared.DTO.Conversations;
using Vibechat.Shared.DTO.Users;

namespace Vibechat.BusinessLogic.Services.Chat
{
    public class ChatService
    {
        private const int MaxThumbnailLengthMb = 5;

        private const int MaxNameLength = 200;

        private const int MaxParticipantsToReturn = 200;

        private readonly BansService bansService;
        
        private readonly MessagesService messagesService;

        private readonly IComparer<Shared.DTO.Conversations.Chat> chatComparer;
        private readonly IRolesRepository staticRolesRepo;

        private readonly IChatDataProvider chatDataProvider;

        private readonly IConversationRepository conversationRepository;

        private readonly FilesService filesService;

        private readonly ILastMessagesRepository lastMessagesRepository;

        private readonly IDhPublicKeysRepository publicKeys;

        private readonly IChatRolesRepository rolesRepository;

        private readonly UnitOfWork unitOfWork;

        private readonly IUsersConversationsRepository usersConversationsRepository;

        private readonly IUsersRepository usersRepository;

        public ChatService(
            IChatDataProvider chatDataProvider,
            IUsersRepository usersRepository,
            IUsersConversationsRepository usersConversationsRepository,
            IConversationRepository conversationRepository,
            ILastMessagesRepository lastMessagesRepository,
            IDhPublicKeysRepository dh,
            IChatRolesRepository rolesRepository,
            FilesService filesService,
            UnitOfWork unitOfWork,
            BansService bansService,
            MessagesService messagesService,
            IComparer<Shared.DTO.Conversations.Chat> chatComparer,
            IRolesRepository staticRolesRepo)
        { 
            this.chatDataProvider = chatDataProvider;
            this.usersRepository = usersRepository;
            this.usersConversationsRepository = usersConversationsRepository;
            this.conversationRepository = conversationRepository;
            this.lastMessagesRepository = lastMessagesRepository;
            publicKeys = dh;
            this.rolesRepository = rolesRepository;
            this.filesService = filesService;
            this.unitOfWork = unitOfWork;
            this.bansService = bansService;
            this.messagesService = messagesService;
            this.chatComparer = chatComparer;
            this.staticRolesRepo = staticRolesRepo;
        }

        public async Task UpdateAuthKey(int chatId, string authKeyId, string deviceId, string thisUserId)
        {
            if (!await ExistsInConversation(chatId, thisUserId))
            {
                throw new UnauthorizedAccessException("Wrong conversation id was provided.");
            }

            var chat = await conversationRepository.GetByIdAsync(chatId);

            if (chat == null)
            {
                throw new KeyNotFoundException("Wrong conversation id was provided.");
            }

            chat.AuthKeyId = authKeyId;
            await conversationRepository.UpdateAsync(chat);

            var userChatEntry = await usersConversationsRepository.GetByIdAsync(thisUserId, chatId);
            userChatEntry.DeviceId = deviceId;
            await usersConversationsRepository.UpdateAsync(userChatEntry);

            await unitOfWork.Commit();
        }

        public async Task<Shared.DTO.Conversations.Chat> CreateConversation(
            string chatName,
            string creatorId,
            string dialogUserId,
            string chatImageUrl,
            bool isGroup,
            bool isPublic,
            bool isSecure,
            string deviceId)
        {
            var defaultError = new InvalidDataException("Error while creating the conversation..");

            //if there was no group info or creator info

            var user = await usersRepository.GetByIdAsync(creatorId);

            if (user == null)
            {
                throw new KeyNotFoundException("Wrong creatorId.");
            }

            if (!isGroup && dialogUserId == null)
            {
                throw new InvalidDataException("No dialog user was provided.");
            }

            if (isGroup && chatName.Length > MaxNameLength)
            {
                throw new InvalidDataException("Chat name was too long.");
            }

            async Task<ConversationDataModel> CreateDialog()
            {
                //if this is a dialogue , find a user with whom to create conversation
                var secondDialogueUser = await usersRepository.GetByIdAsync(dialogUserId);

                if (secondDialogueUser == null)
                {
                    throw defaultError;
                }
                
                DhPublicKeyDataModel dhPublicKey = null;
                
                if (isSecure)
                {
                    dhPublicKey = await publicKeys.GetRandomKey();
                }
                
                var newChat = new ConversationDataModel
                {
                    IsGroup = isGroup,
                    Name = isGroup ? chatName : null,
                    FullImageUrl = null,
                    ThumbnailUrl = null,
                    IsPublic = isPublic,
                    IsSecure = isSecure,
                    PublicKeyId = dhPublicKey?.Id,
                    PublicKey = dhPublicKey
                };
                
                await conversationRepository.AddAsync(newChat);
                
                await usersConversationsRepository.AddAsync(
                    UsersConversationDataModel.Create(dialogUserId, newChat)
                );
                
                await usersConversationsRepository.AddAsync(UsersConversationDataModel.Create(
                    creatorId, 
                    newChat, 
                    deviceId));

                return newChat;
            }

            async Task<ConversationDataModel> CreateGroup()
            {
                var imageUrl = chatImageUrl ?? chatDataProvider.GetGroupPictureUrl();
                
                var newChat = new ConversationDataModel
                {
                    IsGroup = isGroup,
                    Name = isGroup ? chatName : null,
                    FullImageUrl = imageUrl,
                    ThumbnailUrl = imageUrl,
                    IsPublic = isPublic,
                    IsSecure = isSecure,
                    PublicKeyId = null,
                    PublicKey = null
                };
                
                await conversationRepository.AddAsync(newChat);
                
                await usersConversationsRepository.AddAsync(UsersConversationDataModel.Create(
                    creatorId, 
                    newChat, 
                    deviceId));

                await rolesRepository.AddAsync(
                    ChatRoleDataModel.Create(newChat, creatorId, ChatRole.Creator));

                return newChat;
            }

            try
            {
                ConversationDataModel createdChat;
                
                if (isGroup)
                {
                    createdChat = await CreateGroup();
                }
                else
                {
                    createdChat = await CreateDialog();
                }

                await unitOfWork.Commit();
                
                var creator = user.ToAppUserDto();
                creator.ChatRole = new ChatRoleDto
                {
                    ChatId = createdChat.Id,
                    Role = ChatRole.Creator
                };

                return createdChat.ToChatDto(
                    isGroup ? new List<AppUserDto> {creator} : null,
                    isGroup ? null : await usersConversationsRepository.GetUserInDialog(createdChat.Id, creatorId).ConfigureAwait(false),
                    createdChat.PublicKey,
                    !isGroup ? null : await rolesRepository.GetByIdAsync(createdChat.Id, creatorId).ConfigureAwait(false),
                    deviceId,
                    0,
                    null
                );
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Couldn't create group/dialog. Probably there exists one already.", ex);
            }
        }
        
        public async Task<List<AppUserDto>> FindUsersInChat(int chatId, string username, string caller)
        {
            if (!await usersConversationsRepository.Exists(caller, chatId))
            {
                throw new UnauthorizedAccessException("Caller of this method must be the part of conversation.");
            }

            var result = (await usersConversationsRepository.FindUsersInChat(chatId, username))
                .Select(x => x.ToAppUserDto())
                .ToList();
            
            foreach (var user in result)
            {
                user.IsBlockedInConversation =
                    await bansService.IsBannedFromConversation(chatId, user.Id);
                user.ChatRole = await GetChatRole(user.Id, chatId);
            }

            return result;
        }


        public async Task<UpdateThumbnailResponse> UpdateThumbnail(int conversationId, IFormFile image, string userId)
        {
            if (image.Length / (1024 * 1024) > MaxThumbnailLengthMb)
            {
                throw new InvalidDataException($"Thumbnail was larger than {MaxThumbnailLengthMb}");
            }

            var conversation = await conversationRepository.GetByIdAsync(conversationId);

            if (conversation == null)
            {
                throw new InvalidDataException("Wrong conversation id was provided.");
            }

            try
            {
                var (thumbnail, fullsized) = await filesService.SaveProfileOrChatPicture(image, image.FileName,
                    conversationId.ToString(), userId);

                conversation.ThumbnailUrl = thumbnail;
                conversation.FullImageUrl = fullsized;

                await conversationRepository.UpdateAsync(conversation);

                await unitOfWork.Commit();

                return new UpdateThumbnailResponse
                {
                    ThumbnailUrl = conversation.ThumbnailUrl,
                    FullImageUrl = conversation.FullImageUrl
                };
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Failed to update chat image. Try different one.", ex);
            }
        }

        public async Task ChangeName(int conversationId, string name)
        {
            if (name.Length > MaxNameLength)
            {
                throw new InvalidDataException($"Name length couldn't be more than {MaxNameLength}.");
            }

            var conversation = await conversationRepository.GetByIdAsync(conversationId);

            if (conversation == null)
            {
                throw new InvalidDataException("Wrong conversation id was provided.");
            }

            conversation.Name = name;
            await conversationRepository.UpdateAsync(conversation);
            await unitOfWork.Commit();
        }

        public async Task<List<Shared.DTO.Conversations.Chat>> SearchForGroups(string name, string whoAccessedId)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidDataException("Search string coudn't be null.");
            }

            var groups = await conversationRepository.ListAsync(new ChatsByNameSpec(name));

            if (groups == null)
            {
                return null;
            }

            var result = new List<Shared.DTO.Conversations.Chat>();

            foreach (var conversation in groups)
            {
                result.Add(
                    conversation.ToChatDto(
                        await GetParticipants(conversation.Id, MaxParticipantsToReturn),
                        null,
                        conversation.PublicKey,
                        await rolesRepository.GetByIdAsync(conversation.Id, whoAccessedId),
                        null,
                        0,
                        null
                    ));
            }
    
            return result;
        }

        public async Task ChangePublicState(int conversationId, string whoAccessedId)
        {
            var conversation = await conversationRepository.GetByIdAsync(conversationId);

            if (conversation == null)
            {
                throw new InvalidDataException("Wrong chatId.");
            }
            
            var userRole = await rolesRepository.GetByIdAsync(conversationId, whoAccessedId);

            if (userRole.RoleId != ChatRole.Moderator && userRole.RoleId != ChatRole.Creator)
            {
                throw new UnauthorizedAccessException("This method is only allowed to group creator / moderator.");
            }
            
            conversation.IsPublic = !conversation.IsPublic;
            await conversationRepository.UpdateAsync(conversation);
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
            var conversation = await conversationRepository.GetByIdAsync(chatId);

            if (conversation == null)
            {
                throw new InvalidDataException("Wrong conversation.");
            }

            var userConversation = await usersConversationsRepository.GetByIdAsync(userId, chatId);

            if (userConversation == null)
            {
                throw new InvalidDataException("User is not a part of this conversation.");
            }

            var userRole = await rolesRepository.GetByIdAsync(chatId, whoRemovedId);

            if (!IsSelf)
            {
                if (userRole.RoleId != ChatRole.Moderator && userRole.RoleId != ChatRole.Creator)
                {
                    throw new InvalidDataException("Only creator / moderator can remove users in group.");
                }
            }

            await usersConversationsRepository.DeleteAsync(userConversation);

            if ((await usersConversationsRepository.CountAsync(new GetParticipantsSpec(conversation.Id)))
                .Equals(0))
            {
                await conversationRepository.DeleteAsync(conversation);
            }

            await rolesRepository.DeleteAsync(await rolesRepository.GetByIdAsync(chatId, userId));

            await unitOfWork.Commit();
        }

        public async Task RemoveChat(Shared.DTO.Conversations.Chat chat, string whoRemoves)
        {
            var conversation = await conversationRepository.GetByIdAsync(chat.Id);

            if (conversation == null)
            {
                throw new InvalidDataException("Wrong conversation to remove.");
            }

            //secure chats may be removed by any of participants.

            var userRole = await rolesRepository.GetByIdAsync(conversation.Id, whoRemoves);

            if (!conversation.IsSecure)
            {
                if (conversation.IsGroup)
                {
                    if (userRole.RoleId != ChatRole.Creator)
                    {
                        throw new InvalidDataException("Only creator can remove group.");
                    }
                }
            }

            var attachmentsToDelete = await messagesService.GetAllAttachments(conversation.Id, whoRemoves);

            if (attachmentsToDelete != null && attachmentsToDelete.Any())
            {
                filesService.DeleteFiles(attachmentsToDelete
                    .Select(x => x.AttachmentInfo.ContentUrl)
                    .ToList());
            }
            
            //this removes all messages and users-chats links
            await conversationRepository.DeleteAsync(conversation);
            await unitOfWork.Commit();
        }

        public async Task ChangeUserRole(int chatId, string userId, string caller, ChatRole newRole)
        {
            var callerRole = await rolesRepository.GetByIdAsync(chatId, caller);

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

            var targetUserRole = await rolesRepository.GetByIdAsync(chatId, userId);

            var staticRole = staticRolesRepo.Get(newRole);

            targetUserRole.Role = staticRole;

            await rolesRepository.UpdateAsync(targetUserRole);

            await unitOfWork.Commit();
        }

        public async Task<AppUserDto> AddUserToChat(int chatId, string userId)
        {
            var defaultError = new InvalidDataException("Invalid credentials were provided.");

            var FoundConversation = await conversationRepository.GetByIdAsync(chatId);

            if (FoundConversation == null)
            {
                throw defaultError;
            }

            var FoundUser = await usersRepository.GetByIdAsync(userId);

            if (FoundUser == null)
            {
                throw defaultError;
            }

            if (await usersConversationsRepository.Exists(FoundUser.Id, FoundConversation.Id))
            {
                throw new InvalidDataException("User already exists in converation.");
            }

            var addedUser = (await usersConversationsRepository.AddAsync(UsersConversationDataModel.Create(userId, chatId))).User;

            await rolesRepository.AddAsync(ChatRoleDataModel.Create(chatId, userId, ChatRole.NoRole));

            await unitOfWork.Commit();
            return addedUser.ToAppUserDto();
        }

        public async Task ValidateDialog(string firstUserId, string secondUserId)
        {
            if ((await usersConversationsRepository.GetDialog(firstUserId, secondUserId)) == null)
            {
                throw new InvalidDataException("Wrong id of a user in dialog.");
            }
        }

        public async Task<ChatRoleDto> GetChatRole(string userid, int chatId)
        {
            return (await rolesRepository.GetByIdAsync(chatId, userid)).ToChatRole();
        }
        
        /// <summary>
        /// Returns chats sorted by most recent last message.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="whoAccessedId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException"></exception>
        public async Task<List<Shared.DTO.Conversations.Chat>> GetChats(string deviceId, string whoAccessedId)
        {
            var user = await usersRepository.GetByIdAsync(whoAccessedId);

            if (user == null)
            {
                throw new KeyNotFoundException("Can't get chats for this user.");
            }

            var chats = await usersConversationsRepository.GetUserChats(deviceId, whoAccessedId);

            var returnData = new List<Shared.DTO.Conversations.Chat>();

            foreach (var chat in chats)
            {
                var lastMessage = await messagesService.GetLastRecentMessage(chat.Id, whoAccessedId);

                var dtoChat = chat.ToChatDto(
                    chat.IsGroup ? await GetParticipants(chat.Id, MaxParticipantsToReturn) : null,
                    chat.IsGroup
                        ? null
                        :await usersConversationsRepository.GetUserInDialog(chat.Id, whoAccessedId),
                    chat.PublicKey,
                    await rolesRepository.GetByIdAsync(chat.Id, whoAccessedId),
                    chat.IsSecure ? await GetDeviceId(chat.Id, whoAccessedId) : null,
                    (await lastMessagesRepository.GetByIdAsync(whoAccessedId, chat.Id))?.MessageID ?? 0,
                    lastMessage
                );

                dtoChat.IsMessagingRestricted =
                    await bansService.IsBannedFromConversation(chat.Id, whoAccessedId);

                dtoChat.MessagesUnread = await messagesService.GetUnreadMessagesAmount(dtoChat, whoAccessedId);
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
            var chat = await usersConversationsRepository.GetByIdAsync(userId, chatId);
            return chat.DeviceId;
        }

        public async Task<List<AppUserDto>> GetParticipants(int chatId, int maximum = 0)
        {
            var defaultErrorMessage = new InvalidDataException("Wrong conversation was provided.");

            var conversation = await conversationRepository.GetByIdAsync(chatId);

            if (conversation == null)
            {
                throw defaultErrorMessage;
            }

            var participants = await usersConversationsRepository.GetChatParticipants(chatId);

            var users = (from participant in participants
                         select participant.ToAppUserDto()
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
        public async Task<Shared.DTO.Conversations.Chat> GetById(int conversationId, string whoAccessedId)
        {
            var conversation = await conversationRepository.GetByIdAsync(conversationId);

            if (conversation == null)
            {
                throw new KeyNotFoundException("No such conversation was found.");
            }

            var dialogUser = new AppUser();
            var members = new List<AppUserDto>();

            if (conversation.IsGroup)
            {
                members = (await usersConversationsRepository.GetChatParticipants(conversationId))
                    .Select(x => x.ToAppUserDto())
                    .ToList();

                if (!conversation.IsPublic && members.All(usr => usr.Id != whoAccessedId))
                {
                    throw new UnauthorizedAccessException("You are not allowed to view this chat.");
                }
            }
            else
            {
                dialogUser = await usersConversationsRepository.GetUserInDialog(conversationId, whoAccessedId);

                if (dialogUser == null)
                {
                    throw new InvalidDataException("Unexpected error: no corresponding user in dialogue.");
                }
            }
            

            var dtoChat = conversation.ToChatDto(
                members,
                dialogUser,
                conversation.PublicKey,
                await rolesRepository.GetByIdAsync(conversation.Id, whoAccessedId),
                null,
                0,
                null);

            dtoChat.IsMessagingRestricted = await bansService.IsBannedFromConversation(dtoChat.Id, whoAccessedId);
            dtoChat.ClientLastMessageId = (await lastMessagesRepository.GetByIdAsync(whoAccessedId, conversationId))?.MessageID ?? 0;
            
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

        public async Task<Shared.DTO.Conversations.Chat> GetByIdSimplified(int conversationId, string whoAccessedId)
        {
            var conversation = await conversationRepository.GetByIdAsync(conversationId);

            if (conversation == null)
            {
                throw new InvalidDataException("No such conversation was found.");
            }

            var dialogUser = new AppUser();

            if (!conversation.IsGroup)
            {
                dialogUser = await usersConversationsRepository.GetUserInDialog(conversationId, whoAccessedId);

                if (dialogUser == null)
                {
                    throw new InvalidDataException("Unexpected error: no corresponding user in dialogue.");
                }
            }

            return new Shared.DTO.Conversations.Chat
            {
                DialogueUser = dialogUser?.ToAppUserDto(),
                IsGroup = conversation.IsGroup
            };
        }
    }
}