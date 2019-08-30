using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public class ConversationsRepository : BaseRepository<ConversationDataModel>, IConversationRepository
    {
        public ConversationsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

    }
}