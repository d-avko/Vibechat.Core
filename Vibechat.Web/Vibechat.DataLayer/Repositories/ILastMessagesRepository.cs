using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface ILastMessagesRepository : IAsyncRepository<LastMessageDataModel>
    {
        Task<LastMessageDataModel> GetByIdAsync(string userId, int chatId);
    }
}