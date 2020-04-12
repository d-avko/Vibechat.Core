using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vibechat.DataLayer.DataModels;
using Vibechat.DataLayer.Repositories.Specifications.Messages;

namespace Vibechat.DataLayer.Repositories
{
    public class ConversationsRepository : BaseRepository<ConversationDataModel>, IConversationRepository
    {
        private readonly IMessagesRepository messages;

        public ConversationsRepository(ApplicationDbContext dbContext, IMessagesRepository messages) : base(dbContext)
        {
            this.messages = messages;
        }

        public async Task<ConversationDataModel> GetByIdAsync(int id, string userId, int maxParticipants = 100)
        {
            var result = await _dbContext
                .Conversations
                .Include(x => x.Participants)
                    .ThenInclude(x => x.User)
                .Include(x => x.Roles)
                    .ThenInclude(x => x.Role)
                .Include(x => x.BannedUsers)
                .Include(x => x.PublicKey)
                .Include(x => x.LastMessages)
                    .ThenInclude(x => x.Message)
                        .ThenInclude(x => x.Event.Actor)
                .Include(x => x.LastMessages)
                    .ThenInclude(x => x.Message)
                        .ThenInclude(x => x.Event.UserInvolved)
                .Where(x => x.Id == id)
                .ToListAsync();

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
                
                chat.LastMessage = await messages.SingleOrDefaultAsync(new GetMostRecentMessageSpec(chat.Id));
                chat.UnreadCount = await messages.CountAsync(new UnreadMessagesCountSpec(userId, chat.Id, chat.ClientLastMessage));
                
                chat.IsMessagingRestricted = chat.BannedUsers?.Any(x => x.UserID == userId) ?? false;
                chat.Role = chat.Roles?.FirstOrDefault(x => x.UserId == userId);
            }

            return result.ToList().SingleOrDefault();
        }

        public async Task<List<ConversationDataModel>> GetChatsByName(string name, int maxParticipants = 100)
        {
            var result = _dbContext
                .Conversations
                .Include(x => x.Roles)
                .Include(x => x.Participants)
                .ThenInclude(x => x.User)
                .Where(chat => chat.IsPublic &&
                               EF.Functions.Like(chat.Name, name + "%"));

            foreach (var chat in result)
            {
                chat.participants = chat.Participants.Take(maxParticipants).Select(x => x.User);
            }

            return await result.ToListAsync();
        }

    }
}