using System.Threading.Tasks;
using Vibechat.Web.Data_Layer.DataModels;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public interface ILastMessagesRepository : IAsyncRepository<LastMessageDataModel>
    {
        Task<LastMessageDataModel> GetByIdAsync(string userId, int chatId);
    }
}