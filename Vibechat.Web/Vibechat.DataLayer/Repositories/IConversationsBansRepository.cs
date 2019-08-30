using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface IConversationsBansRepository : IAsyncRepository<ConversationsBansDataModel>
    {
        Task<ConversationsBansDataModel> GetByIdAsync(string userId, int chatId);
    }
}