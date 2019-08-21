using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public interface IDeletedMessagesRepository : IAsyncRepository<DeletedMessagesDataModel>
    {
        IQueryable<DeletedMessagesDataModel> GetAllAsQuerable(string userId);
    }
}
