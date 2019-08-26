using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Vibechat.Web.Data_Layer.Repositories;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public interface IUsersRepository
    {
        Task<IdentityResult> CreateUser(AppUser user);

        Task DisableUserLockout(AppUser user);

        Task LockoutUser(AppUser user, DateTimeOffset until);

        Task<IQueryable<AppUser>> FindByUsername(string username);
        Task<AppUser> GetByUsername(string username);
        Task<AppUser> GetById(string id);
        Task MakeUserOffline(string userId);
        Task MakeUserOnline(string userId, bool updateConnectionId = false, string signalRConnectionId = null);

        Task ChangeUserPublicState(string userId);

        Task UpdateAvatar(string thumbnail, string fullSized, string userId);

        Task ChangeLastName(string newName, string userId);

        Task ChangeName(string newName, string userId);

        Task ChangeUsername(string newName, string userId);

        Task<string> GetRefreshToken(string userId);

        Task UpdateRefreshToken(string userId, string token);
    }
}