using System.Linq;
using Microsoft.EntityFrameworkCore;
using Vibechat.Web.Data_Layer.Repositories;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public class ConversationsRepository : BaseRepository<ConversationDataModel>, IConversationRepository
    {
        public ConversationsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

    }
}