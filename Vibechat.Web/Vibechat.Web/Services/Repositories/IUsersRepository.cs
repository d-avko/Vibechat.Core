using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IUsersRepository
    {
        Task<bool> CheckPassword(string password, UserInApplication user);

        Task<IdentityResult> CreateUser(UserInApplication user, string password);
        Task<IQueryable<UserInApplication>> FindByUsername(string username);
        Task<UserInApplication> GetByUsername(string username);
        Task<UserInApplication> GetByEmail(string email);
        Task<UserInApplication> GetById(string id);
        Task MakeUserOffline(string userId);
        Task MakeUserOnline(string userId, string signalRConnectionId);
        Task ChangeUserPublicState(string userId);

        Task UpdateThumbnail(string thumbnail, string fullSized, string userId);

        Task ChangeLastName(string newName, string userId);

        Task ChangeName(string newName, string userId);

        Task<string> GetRefreshToken(string userId);

        Task UpdateRefreshToken(string userId, string token);
    }
}