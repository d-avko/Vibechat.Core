using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface IUsersBansRepository : IAsyncRepository<UsersBansDatamodel>
    {
        Task<UsersBansDatamodel> GetByIdAsync(string userId, string whoUnbansId);
        Task<bool> IsBanned(string whoId, string byWhomId);
    }
}