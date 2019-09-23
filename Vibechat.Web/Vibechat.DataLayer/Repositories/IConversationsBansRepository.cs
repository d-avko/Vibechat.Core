using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface IConversationsBansRepository : IAsyncRepository<ConversationsBansDataModel>
    {
        ValueTask<ConversationsBansDataModel> GetByIdAsync(string userId, int chatId);
    }
}