using VibeChat.Web;
using Vibechat.Web.Data_Layer.DataModels;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public class ChatEventsRepository : IChatEventsRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ChatEventsRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public ChatEventDataModel Add(ChatEventDataModel model)
        {
            return dbContext.ChatEvents.Add(model)?.Entity;
        }
    }
}