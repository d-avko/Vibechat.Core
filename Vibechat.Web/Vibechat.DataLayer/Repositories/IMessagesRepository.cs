using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface IMessagesRepository : IAsyncRepository<MessageDataModel>
    {       
        List<MessageDataModel> Search
            (int offset, int count, string searchString, string userId);

        Task<MessageDataModel> GetMostRecentMessage(int conversationId);

        Task<int> GetUnreadMessagesCount(int chatId, int lastMessageId, string userId);
    }
}