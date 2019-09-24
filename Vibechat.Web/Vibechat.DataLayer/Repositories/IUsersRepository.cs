using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Vibechat.DataLayer.Repositories
{
    public interface IUsersRepository
    {
        Task<IdentityResult> CreateUser(AppUser user);

        Task UpdateAsync(AppUser user);

        Task DisableUserLockout(AppUser user);

        Task LockoutUser(AppUser user, DateTimeOffset until);

        Task<IQueryable<AppUser>> FindByUsername(string username);
        Task<AppUser> GetByUsername(string username);
        Task<AppUser> GetByIdAsync(string id);
        Task MakeUserOffline(AppUser user);

        Task MakeUserOnline(AppUser user);

        Task ChangeUserPublicState(AppUser user);

        Task UpdateAvatar(string thumbnail, string fullSized, AppUser user);

        Task ChangeLastName(string newName, AppUser user);

        Task ChangeName(string newName, AppUser user);

        Task ChangeUsername(string newName, AppUser user);

        Task UpdateRefreshToken(AppUser user, string token);
    }
}