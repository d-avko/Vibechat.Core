using Vibechat.DataLayer.DataModels;
using Vibechat.Shared.DTO.Conversations;

namespace Vibechat.DataLayer.Repositories
{
    public interface IRolesRepository
    {
        RoleDataModel Get(ChatRole role);
    }
}