using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public class ConversationsBansRepository : BaseRepository<ConversationsBansDataModel>, IConversationsBansRepository
    {
        public ConversationsBansRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            
        }

        public Task<ConversationsBansDataModel> GetByIdAsync(string userId, int chatId)
        {
            return _dbContext.ConversationsBans.FindAsync(chatId, userId);
        }
    }
}