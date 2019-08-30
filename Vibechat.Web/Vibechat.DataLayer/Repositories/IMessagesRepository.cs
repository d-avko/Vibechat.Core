using System.Collections.Generic;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface IMessagesRepository : IAsyncRepository<MessageDataModel>
    {       
        List<MessageDataModel> Search
            (int offset, int count, string searchString, string userId);
    }
}