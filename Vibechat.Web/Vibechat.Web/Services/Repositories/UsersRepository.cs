using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private UserManager<AppUser> mUserManager { get; set; }

        public UsersRepository(UserManager<AppUser> mUserManager)
        {
            this.mUserManager = mUserManager;
        }

        public async Task MakeUserOnline(string userId, string signalRConnectionId)
        {
            AppUser user = await GetById(userId);

            user.IsOnline = true;

            user.LastSeen = DateTime.UtcNow;

            user.ConnectionId = signalRConnectionId;
        }

        public async Task MakeUserOnline(string userId)
        {
            AppUser user = await GetById(userId);

            user.IsOnline = true;

            user.LastSeen = DateTime.UtcNow;
        }

        public async Task MakeUserOffline(string userId)
        {
            var user = await GetById(userId);

            user.IsOnline = false;

            user.ConnectionId = null;
        }

        public Task<AppUser> GetById(string id)
        {
            return mUserManager.FindByIdAsync(id);
        }

        public Task<AppUser> GetByEmail(string email)
        {
            return mUserManager.FindByEmailAsync(email);
        }

        public Task<AppUser> GetByUsername(string username)
        {
            return mUserManager.FindByNameAsync(username);
        }

        public Task<bool> CheckPassword(string password, AppUser user)
        {
           return mUserManager.CheckPasswordAsync(user, password);
        }

        public Task<IdentityResult> CreateUser(AppUser user, string password)
        {
            return mUserManager.CreateAsync(user, password);
        }

        public Task<IdentityResult> CreateUser(AppUser user)
        {
            return mUserManager.CreateAsync(user);
        }

        public Task<IdentityResult> DeleteUser(AppUser user)
        {
            return mUserManager.DeleteAsync(user);
        }

        public async Task<IQueryable<AppUser>> FindByUsername(string username)
        {
            return mUserManager
                .Users
                .Where(user => user.IsPublic)
                .Where(user => user.UserName.StartsWith(username, StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task ChangeUserPublicState(string userId)
        {
            var user = await GetById(userId);
            user.IsPublic = !user.IsPublic;
        }

        public async Task ChangeName(string newName, string userId)
        {
            var user = await GetById(userId);
            user.FirstName = newName;
        }

        public async Task ChangeLastName(string newName, string userId)
        {
            var user = await GetById(userId);
            user.LastName = newName;
        }

        public async Task ChangeUsername(string newName, string userId)
        {
            var user = await GetById(userId);
            user.UserName = newName;
            await mUserManager.UpdateAsync(user);
        }

        public async Task<string> GetRefreshToken(string userId)
        {
            var user = await GetById(userId);
            return user.RefreshToken;
        }

        public async Task UpdateRefreshToken(string userId, string token)
        {
            var user = await GetById(userId);
            user.RefreshToken = token;
        }

        public async Task UpdateAvatar(string thumbnail, string fullSized, string userId)
        {
            var user = await GetById(userId);
            user.ProfilePicImageURL = thumbnail;
            user.FullImageUrl = fullSized;
        }
    }
}
