using System.Linq;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public class DeletedMessagesRepository : BaseRepository<DeletedMessagesDataModel>, IDeletedMessagesRepository
    {
        public DeletedMessagesRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<DeletedMessagesDataModel> GetAllAsQuerable(string userId)
        {
            return _dbContext.DeletedMessages.Where(msg => msg.UserId == userId).AsQueryable();
        }
    }
}
