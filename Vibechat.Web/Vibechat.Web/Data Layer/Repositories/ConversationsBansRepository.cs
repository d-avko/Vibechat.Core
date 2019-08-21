using System.Linq;
using VibeChat.Web;
using Vibechat.Web.Data.DataModels;
using Vibechat.Web.Data_Layer.Repositories;
using System.Threading.Tasks;

namespace Vibechat.Web.Data.Repositories
{
    public class ConversationsBansRepository : BaseRepository<ConversationsBansDataModel>, IConversationsBansRepository
    {
        public ConversationsBansRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            
        }

        public ValueTask<ConversationsBansDataModel> GetByIdAsync(string userId, int chatId)
        {
            return _dbContext.ConversationsBans.FindAsync(chatId, userId);
        }
    }
}