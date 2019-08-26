using System.Collections.Generic;
using System.Threading.Tasks;
using VibeChat.Web;
using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Data.DataModels;
using Vibechat.Web.Data_Layer.Repositories;

namespace Vibechat.Web.Data.Repositories
{
    public interface IChatRolesRepository : IAsyncRepository<ChatRoleDataModel>
    {
        Task<ChatRoleDataModel> GetByIdAsync(int chatId, string userId);
    }
}