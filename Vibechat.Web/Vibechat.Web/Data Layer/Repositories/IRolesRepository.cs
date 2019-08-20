using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public interface IRolesRepository
    {
        RoleDataModel Get(ChatRole role);
    }
}