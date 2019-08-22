using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vibechat.Web.Data_Layer.Repositories;
using Vibechat.Web.Data_Layer.Repositories.Specifications.UsersChats;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public class UsersConversationsRepository : BaseRepository<UsersConversationDataModel>, IUsersConversationsRepository
    {
        public UsersConversationsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            
        }

        public ValueTask<UsersConversationDataModel> GetByIdAsync(string userId, int conversationId)
        {
            return _dbContext
                .UsersConversations
                .FindAsync(userId, conversationId);
        }

        public async Task<IEnumerable<ConversationDataModel>> GetUserChats(string deviceId, string userId)
        {
           return (await ListAsync(new GetUserChatsSpec(deviceId, userId)))
                .Select(x => x.Conversation);
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
            return (await base.ListAsync(new GetUserInDialogSpec(chatId, firstUserInDialog)))
                .First()
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