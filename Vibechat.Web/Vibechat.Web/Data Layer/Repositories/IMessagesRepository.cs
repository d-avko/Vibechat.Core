using System.Collections.Generic;
using System.Linq;
using VibeChat.Web;
using Vibechat.Web.Data.Messages;
using Vibechat.Web.Data_Layer.Repositories;

namespace Vibechat.Web.Data.Repositories
{
    public interface IMessagesRepository : IAsyncRepository<MessageDataModel>
    {       
        List<MessageDataModel> Search
            (int offset, int count, string searchString, string userId);
    }
}