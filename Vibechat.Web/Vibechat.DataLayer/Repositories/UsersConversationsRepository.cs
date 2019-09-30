using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;
using Vibechat.DataLayer.Repositories.Specifications.Messages;
using Vibechat.DataLayer.Repositories.Specifications.UsersChats;

namespace Vibechat.DataLayer.Repositories
{
    public class UsersConversationsRepository : BaseRepository<UsersConversationDataModel>, IUsersConversationsRepository
    {
        private readonly IMessagesRepository messages;
        private readonly IConversationsBansRepository conversationsBans;

        public UsersConversationsRepository(ApplicationDbContext dbContext, 
            IMessagesRepository messages, IConversationsBansRepository conversationsBans) : base(dbContext)
        {
            this.messages = messages;
            this.conversationsBans = conversationsBans;
        }

        public ValueTask<UsersConversationDataModel> GetByIdAsync(string userId, int conversationId)
        {
            return _dbContext
                .UsersConversations
                .FindAsync(userId, conversationId);
        }

        public async Task<IEnumerable<ConversationDataModel>> GetUserChats(string deviceId, string userId, int maxParticipants = 100)
        {
            var result = _dbContext
                 .UsersConversations
                 .Include(x => x.Conversation.Participants)
                     .ThenInclude(x => x.User)
                 .Include(x => x.Conversation.Roles)
                    .ThenInclude(x => x.Role)
                 .Include(x => x.Conversation.BannedUsers)
                 .Include(x => x.Conversation.PublicKey)
                 .Include(x => x.Conversation.LastMessages)
                     .ThenInclude(x => x.Message)
                         .ThenInclude(x => x.Event.Actor)
                 .Include(x => x.Conversation.LastMessages)
                     .ThenInclude(x => x.Message)
                         .ThenInclude(x => x.Event.UserInvolved)
                 .Where(chat => chat.UserID == userId && (chat.DeviceId == null || chat.DeviceId == deviceId))
                 .Select(x => x.Conversation);

            foreach (var chat in result)
            {
                if (chat.Participants != null)
                {
                    chat.participants = chat.Participants.Take(maxParticipants).Select(x => x.User);

                    foreach (var p in chat.participants)
                    {
                        p.ChatRole = chat.Roles.FirstOrDefault(x => x.UserId == p.Id);
                        p.IsBlockedInChat = chat.BannedUsers.FirstOrDefault(x => x.UserID == p.Id) != null;
                    }

                    chat.DeviceId = chat.Participants.FirstOrDefault(x => x.UserID == userId)?.DeviceId;
                }

                chat.ClientLastMessage =
                    chat.LastMessages?.FirstOrDefault(x => x.UserID == userId)?.Message?.MessageID ?? 0;
                chat.LastMessage = await messages.GetMostRecentMessage(chat.Id);
                chat.UnreadCount = await messages.GetUnreadMessagesCount(chat.Id, chat.LastMessage?.MessageID ?? 0, userId);
                chat.IsMessagingRestricted = chat.BannedUsers?.Any(x => x.UserID == userId) ?? false;
                chat.Role = chat.Roles?.FirstOrDefault(x => x.UserId == userId);
            }

            return await result
                .ToListAsync();
        }

        public async Task<IEnumerable<AppUser>> GetChatParticipants(int chatId)
        {
            return (await ListAsync(new GetParticipantsSpec(chatId))).Select(x => x.User);
        }

        public async Task<IEnumerable<AppUser>> FindUsersInChat(int chatId, string username)
        {
            return (await ListAsync(new FindUsersInChatSpec(username, chatId))).Select(x => x.User);
        }

        public async Task<bool> Exists(string userId, int conversationId)
        {
            return (await GetByIdAsync(userId, conversationId)) != default(UsersConversationDataModel);
        }

        public async Task<AppUser> GetUserInDialog(int chatId, string firstUserInDialog)
        {
            return (await ListAsync(new GetUserInDialogSpec(chatId, firstUserInDialog)))
                .FirstOrDefault()?
                .User;
        }

        public async Task<UsersConversationDataModel> GetDialog(string firstUserId, string secondUserId)
        {
            var result = await ListAsync(new GetDialogsSpec(firstUserId));
            return result?.GroupBy(entry => entry.ChatID)
                .FirstOrDefault(group => group.Any(entry => entry.UserID == firstUserId) 
                                         && group.Any(entry => entry.UserID == secondUserId))
                ?.First();
        }
    }
}