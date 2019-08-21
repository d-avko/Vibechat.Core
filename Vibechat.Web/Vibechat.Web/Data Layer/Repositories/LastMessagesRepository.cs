using System.Linq;
using VibeChat.Web;
using Vibechat.Web.Data_Layer.DataModels;
using System.Threading.Tasks;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public class LastMessagesRepository : BaseRepository<LastMessageDataModel>, ILastMessagesRepository
    {
        public LastMessagesRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            
        }

        public ValueTask<LastMessageDataModel> GetByIdAsync(string userId, int chatId)
        {
            return _dbContext.LastViewedMessages.FindAsync(chatId,userId);
        }
    }
}