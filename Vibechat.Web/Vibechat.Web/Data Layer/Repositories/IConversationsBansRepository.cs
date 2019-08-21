using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using Vibechat.Web.Data_Layer.Repositories;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public interface IConversationsBansRepository : IAsyncRepository<ConversationsBansDataModel>
    {
        ValueTask<ConversationsBansDataModel> GetByIdAsync(string userId, int chatId);
    }
}