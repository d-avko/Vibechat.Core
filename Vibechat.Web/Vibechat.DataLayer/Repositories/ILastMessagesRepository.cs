using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface ILastMessagesRepository : IAsyncRepository<LastMessageDataModel>
    {
        ValueTask<LastMessageDataModel> GetByIdAsync(string userId, int chatId);
    }
}