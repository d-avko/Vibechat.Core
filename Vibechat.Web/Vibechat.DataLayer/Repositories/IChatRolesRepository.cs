using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface IChatRolesRepository : IAsyncRepository<ChatRoleDataModel>
    {
        ValueTask<ChatRoleDataModel> GetByIdAsync(int chatId, string userId);
    }
}