using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using Vibechat.Web.Data_Layer.Repositories;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public interface IUsersBansRepository : IAsyncRepository<UsersBansDatamodel>
    {
        Task<UsersBansDatamodel> GetByIdAsync(string userId, string whoUnbansId);
        Task<bool> IsBanned(string whoId, string byWhomId);
    }
}