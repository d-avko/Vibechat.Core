using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories
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
