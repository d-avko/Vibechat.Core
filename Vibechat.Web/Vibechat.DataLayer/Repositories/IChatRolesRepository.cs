using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface IChatRolesRepository : IAsyncRepository<ChatRoleDataModel>
    {
        Task<ChatRoleDataModel> GetByIdAsync(int chatId, string userId);
    }
}