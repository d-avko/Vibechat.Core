using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vibechat.BusinessLogic.Extensions;
using Vibechat.BusinessLogic.Services.Bans;
using Vibechat.BusinessLogic.Services.ChatDataProviders;
using Vibechat.BusinessLogic.Services.Connections;
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
            var user = await usersRepository.GetByIdAsync(creatorId);
 
            if (user == null)
            {
                throw new KeyNotFoundException("Wrong creatorId.");
            }

            if (isGroup)
            {
                return await CreateGroup(user, chatImageUrl, chatName, isPublic);
            }

            return await CreateDialog(user, dialogUserId, isSecure, deviceId);
        }

        private async Task<Shared.DTO.Conversations.Chat> CreateGroup(
            AppUser creator,
            string chatImageUrl,
            string chatName,
            bool isPublic)
        {
            if (chatName.Length > MaxNameLength)
            {
                throw new InvalidDataException("Chat name was too long.");
            }

            try
            {
                var imageUrl = chatImageUrl ?? chatDataProvider.GetGroupPictureUrl();
                
                var newChat = new ConversationDataModel
                {
                    IsGroup = true,
                    Name = chatName,
                    FullImageUrl = imageUrl,
                    ThumbnailUrl = imageUrl,
                    IsPublic = isPublic
                };
                
                await conversationRepository.AddAsync(newChat);
                
                await usersConversationsRepository.AddAsync(UsersConversationDataModel.Create(
                    creator.Id, 
                    newChat));

                var role = await rolesRepository.AddAsync(
                    ChatRoleDataModel.Create(newChat, creator.Id, ChatRole.Creator));
                
                newChat.Roles = new List<ChatRoleDataModel>(){ role };
                newChat.participants = new List<AppUser> {creator};
                
                await unitOfWork.Commit();

                newChat.Role = new ChatRoleDataModel()
                {
                    ChatId = newChat.Id,
                    RoleId = ChatRole.Creator
                };


                return newChat.ToChatDto(creator.Id);
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Couldn't create group because of unexpected error.", e);
            }
        }

        private async Task<Shared.DTO.Conversations.Chat> CreateDialog(
            AppUser creator,
            string dialogUserId,
            bool isSecure,
            string deviceId)
        {
            var user = await usersRepository.GetByIdAsync(creator.Id);
 
            if (user == null)
            {
                throw new KeyNotFoundException("Wrong creatorId.");
            }

            try
            {
                //if this is a dialogue , find a user with whom to create chat
                var secondDialogueUser = await usersRepository.GetByIdAsync(dialogUserId);

                if (secondDialogueUser == null)
                {
                    throw new InvalidDataException("Wrong dialog user Id.");
                }
                
                DhPublicKeyDataModel dhPublicKey = null;
                
                if (isSecure)
                {
                    dhPublicKey = await publicKeys.GetRandomKey();
                }
                
                var newChat = new ConversationDataModel
                {
                    IsGroup = false,
                    IsSecure = isSecure,
                    PublicKeyId = dhPublicKey?.Id,
                    PublicKey = dhPublicKey,
                    DeviceId = deviceId
                };
                
                await conversationRepository.AddAsync(newChat);
                
                await usersConversationsRepository.AddAsync(
                    UsersConversationDataModel.Create(dialogUserId, newChat)
                );
                
                await usersConversationsRepository.AddAsync(UsersConversationDataModel.Create(
                    creator.Id, 
                    newChat, 
                    deviceId));

                newChat.participants = new List<AppUser> {user, secondDialogueUser};
            
                await unitOfWork.Commit();
                
                return newChat.ToChatDto(creator.Id);
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Couldn't create dialog. Probably there exists one already.", e);
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
                user.IsBlockedInConversation = await bansService.IsBannedFromConversation(chatId, user.Id);
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

            var groups = await conversationRepository.GetChatsByName(name);

            return groups?.Select(x => x.ToChatDto(whoAccessedId)).ToList();
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

            var returnData = chats.Select(x => x.ToChatDto(whoAccessedId)).ToList();

            returnData.Sort(chatComparer);
            
            return returnData;
        }

        public async Task<List<AppUserDto>> GetParticipants(int chatId, int maximum = 0)
        {
            var defaultErrorMessage = new InvalidDataException("Wrong conversation was provided.");

            var conversation = await conversationRepository.GetByIdAsync(chatId);

            if (conversation == null)
            {
                throw defaultErrorMessage;
            }

            var participants = (await usersConversationsRepository.GetChatParticipants(chatId)).ToList();

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
            var existsInChat = await usersConversationsRepository.Exists(whoAccessedId, conversationId);
            
            var conversation = await conversationRepository.GetByIdAsync(conversationId, whoAccessedId);

            if (conversation == null)
            {
                throw new KeyNotFoundException("No such conversation was found.");
            }
            
            if (!existsInChat && !conversation.IsPublic)
            {
                throw new UnauthorizedAccessException("You are not allowed to view this chat.");
            }
            return conversation.ToChatDto(whoAccessedId);
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