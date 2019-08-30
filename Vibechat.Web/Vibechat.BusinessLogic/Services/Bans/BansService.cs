using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;
using Vibechat.DataLayer.Repositories;
using Vibechat.Shared.DTO.Conversations;

namespace Vibechat.BusinessLogic.Services.Bans
{
    public class BansService
    {
        private readonly IChatRolesRepository rolesRepository;
        private readonly UnitOfWork unitOfWork;
        private readonly IUsersConversationsRepository usersConversationsRepository;

        public BansService(
            IUsersBansRepository usersBansRepository,
            IConversationsBansRepository conversationsBansRepository,
            IUsersRepository usersRepository,
            IConversationRepository conversationRepository,
            IUsersConversationsRepository usersConversationsRepository,
            UnitOfWork unitOfWork,
            IChatRolesRepository rolesRepository)
        {
            UsersBansRepository = usersBansRepository;
            ConversationsBansRepository = conversationsBansRepository;
            this.usersRepository = usersRepository;
            ConversationRepository = conversationRepository;
            this.usersConversationsRepository = usersConversationsRepository;
            this.unitOfWork = unitOfWork;
            this.rolesRepository = rolesRepository;
        }

        public IUsersBansRepository UsersBansRepository { get; }
        public IConversationsBansRepository ConversationsBansRepository { get; }
        public IUsersRepository usersRepository { get; }
        public IConversationRepository ConversationRepository { get; }

        public async Task BanUserFromConversation(int conversationId, string userToBanId, string whoAccessedId)
        {
            if (userToBanId == whoAccessedId)
            {
                throw new InvalidDataException("Can't ban yourself.");
            }

            var conversation = await ConversationRepository.GetByIdAsync(conversationId);
            var banned = await usersRepository.GetByIdAsync(userToBanId);

            var userRole = await rolesRepository.GetByIdAsync(conversationId, whoAccessedId);

            if (userRole.RoleId != ChatRole.Moderator && userRole.RoleId != ChatRole.Creator)
            {
                throw new InvalidDataException("Only creator / moderator can ban users.");
            }

            if (banned == null)
            {
                throw new InvalidDataException("Wrong user to ban id was provided.");
            }

            if (conversation == null)
            {
                throw new InvalidDataException("Wrong conversation id was provided.");
            }

            try
            {
                await ConversationsBansRepository.AddAsync(ConversationsBansDataModel.Create(banned, conversation));
                await unitOfWork.Commit();
            }
            catch
            {
                throw new InvalidDataException("Wrong conversation id was provided.");
            }
        }

        public async Task BanDialog(string UserToBanId, string whoAccessedId)
        {
            if (UserToBanId == whoAccessedId)
            {
                throw new InvalidDataException("Can't ban yourself.");
            }

            var bannedBy = await usersRepository.GetByIdAsync(whoAccessedId);
            var banned = await usersRepository.GetByIdAsync(UserToBanId);

            if (banned == null || bannedBy == null)
            {
                throw new KeyNotFoundException("Wrong id of person to ban.");
            }

            try
            {
                await UsersBansRepository.AddAsync(UsersBansDatamodel.Create(banned, bannedBy));
            }
            catch
            {
                throw new InvalidDataException("User is already banned.");
            }

            UsersConversationDataModel dialog;

            if ((dialog = await usersConversationsRepository.GetDialog(UserToBanId, whoAccessedId)) != null)
            {
                await ConversationsBansRepository.AddAsync(ConversationsBansDataModel.Create(banned, dialog.Conversation));
            }

            await unitOfWork.Commit();
        }

        public async Task<bool> IsBannedFromConversation(int conversationId, string Who)
        {
            var who = await usersRepository.GetByIdAsync(Who);

            if (who == null)
            {
                throw new InvalidDataException("Wrong id of a person to check.");
            }

            return (await ConversationsBansRepository.GetByIdAsync(Who, conversationId)) != null;
        }

        public bool IsBannedFromMessagingWith(string who, string byWho)
        {
            // check if allowed
            return UsersBansRepository.IsBanned(who, byWho).GetAwaiter().GetResult();
        }

        public async Task UnbanDialog(string userId, string whoUnbans)
        {
            try
            {
                await UsersBansRepository.DeleteAsync(await UsersBansRepository.GetByIdAsync(userId, whoUnbans));
            }
            catch
            {
                throw new InvalidDataException("Wrong id of a person to unban.");
            }

            UsersConversationDataModel dialog;

            if ((dialog = await usersConversationsRepository.GetDialog(userId, whoUnbans)) != null)
            {
                var entry = await ConversationsBansRepository.GetByIdAsync(userId, dialog.Conversation.Id);

                await ConversationsBansRepository.DeleteAsync(entry);
            }

            await unitOfWork.Commit();
        }

        public async Task UnbanUserFromConversation(int conversationId, string userToUnbanId, string whoAccessedId)
        {
            if (userToUnbanId == whoAccessedId)
            {
                throw new InvalidDataException("Can't unban yourself.");
            }

            var conversation = await ConversationRepository.GetByIdAsync(conversationId);

            var banned = await usersRepository.GetByIdAsync(userToUnbanId);

            var userRole = await rolesRepository.GetByIdAsync(conversationId, whoAccessedId);

            if (userRole.RoleId != ChatRole.Moderator && userRole.RoleId != ChatRole.Creator)
            {
                throw new InvalidDataException("Only creator / moderator can unban users.");
            }

            if (banned == null)
            {
                throw new InvalidDataException("Wrong user to unban id was provided.");
            }

            if (conversation == null)
            {
                throw new InvalidDataException("Wrong conversation id was provided.");
            }

            try
            {
                var entry = await ConversationsBansRepository.GetByIdAsync(userToUnbanId, conversationId);
                await ConversationsBansRepository.DeleteAsync(entry);
                await unitOfWork.Commit();
            }
            catch
            {
                throw new InvalidDataException("Wrong conversation id was provided.");
            }
        }

        public async Task LockoutUser(string userId)
        {
            var user = await usersRepository.GetByIdAsync(userId);
            await usersRepository.LockoutUser(user, DateTimeOffset.UtcNow.AddYears(5));
        }

        public async Task DisableLockout(string userId)
        {
            var user = await usersRepository.GetByIdAsync(userId);
            await usersRepository.DisableUserLockout(user);
        }
    }
}