using System;
using System.Threading.Tasks;
using VibeChat.Web;
using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Data.Repositories;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Services.Bans
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
            UsersRepository = usersRepository;
            ConversationRepository = conversationRepository;
            this.usersConversationsRepository = usersConversationsRepository;
            this.unitOfWork = unitOfWork;
            this.rolesRepository = rolesRepository;
        }

        public IUsersBansRepository UsersBansRepository { get; }
        public IConversationsBansRepository ConversationsBansRepository { get; }
        public IUsersRepository UsersRepository { get; }
        public IConversationRepository ConversationRepository { get; }

        public async Task BanUserFromConversation(int conversationId, string userToBanId, string whoAccessedId)
        {
            if (userToBanId == whoAccessedId)
            {
                throw new FormatException("Can't ban yourself.");
            }

            var conversation = await ConversationRepository.GetByIdAsync(conversationId);
            var banned = await UsersRepository.GetById(userToBanId);

            var userRole = await rolesRepository.GetByIdAsync(conversationId, whoAccessedId);

            if (userRole.RoleId != ChatRole.Moderator && userRole.RoleId != ChatRole.Creator)
            {
                throw new FormatException("Only creator / moderator can ban users.");
            }

            if (banned == null)
            {
                throw new FormatException("Wrong user to ban id was provided.");
            }

            if (conversation == null)
            {
                throw new FormatException("Wrong conversation id was provided.");
            }

            try
            {
                await ConversationsBansRepository.AddAsync(ConversationsBansDataModel.Create(banned, conversation));
                await unitOfWork.Commit();
            }
            catch
            {
                throw new FormatException("Wrong conversation id was provided.");
            }
        }

        public async Task BanDialog(string UserToBanId, string whoAccessedId)
        {
            if (UserToBanId == whoAccessedId)
            {
                throw new FormatException("Can't ban yourself.");
            }

            var bannedBy = await UsersRepository.GetById(whoAccessedId);
            var banned = await UsersRepository.GetById(UserToBanId);

            if (banned == null || bannedBy == null)
            {
                throw new FormatException("Wrong id of person to ban.");
            }

            try
            {
                await UsersBansRepository.AddAsync(UsersBansDatamodel.Create(banned, bannedBy));
            }
            catch
            {
                throw new FormatException("User is already banned.");
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
            var who = await UsersRepository.GetById(Who);

            if (who == null)
            {
                throw new FormatException("Wrong id of a person to check.");
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
                throw new FormatException("Wrong id of a person to unban.");
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
                throw new FormatException("Can't unban yourself.");
            }

            var conversation = await ConversationRepository.GetByIdAsync(conversationId);

            var banned = await UsersRepository.GetById(userToUnbanId);

            var userRole = await rolesRepository.GetByIdAsync(conversationId, whoAccessedId);

            if (userRole.RoleId != ChatRole.Moderator && userRole.RoleId != ChatRole.Creator)
            {
                throw new FormatException("Only creator / moderator can unban users.");
            }

            if (banned == null)
            {
                throw new FormatException("Wrong user to unban id was provided.");
            }

            if (conversation == null)
            {
                throw new FormatException("Wrong conversation id was provided.");
            }

            try
            {
                var entry = await ConversationsBansRepository.GetByIdAsync(userToUnbanId, conversationId);
                await ConversationsBansRepository.DeleteAsync(entry);
                await unitOfWork.Commit();
            }
            catch
            {
                throw new FormatException("Wrong conversation id was provided.");
            }
        }
    }
}