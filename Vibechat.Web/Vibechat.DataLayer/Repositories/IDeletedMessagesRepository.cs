using System.Linq;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface IDeletedMessagesRepository : IAsyncRepository<DeletedMessagesDataModel>
    {
        IQueryable<DeletedMessagesDataModel> GetAllAsQuerable(string userId);
    }
}
