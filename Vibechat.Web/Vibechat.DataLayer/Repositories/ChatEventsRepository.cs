using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public class ChatEventsRepository : BaseRepository<ChatEventDataModel>, IChatEventsRepository
    {
        public ChatEventsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
