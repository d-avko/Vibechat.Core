using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public class LastMessagesRepository : BaseRepository<LastMessageDataModel>, ILastMessagesRepository
    {
        public LastMessagesRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            
        }

        public Task<LastMessageDataModel> GetByIdAsync(string userId, int chatId)
        {
            return _dbContext.LastViewedMessages.FindAsync(chatId,userId);
        }
    }
}