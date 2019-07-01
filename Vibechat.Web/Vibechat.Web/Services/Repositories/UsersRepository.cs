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
        private ApplicationDbContext mContext { get; set; }

        private UserManager<AppUser> mUserManager { get; set; }

        public UsersRepository(ApplicationDbContext dbContext, UserManager<AppUser> mUserManager)
        {
            this.mContext = dbContext;
            this.mUserManager = mUserManager;
        }

        public async Task MakeUserOnline(string userId, string signalRConnectionId)
        {
            AppUser user = await GetById(userId);

            user.IsOnline = true;

            user.LastSeen = DateTime.UtcNow;

            user.ConnectionId = signalRConnectionId;

            await mContext.SaveChangesAsync();
        }

        public async Task MakeUserOnline(string userId)
        {
            AppUser user = await GetById(userId);

            user.IsOnline = true;

            user.LastSeen = DateTime.UtcNow;

            await mContext.SaveChangesAsync();
        }

        public async Task MakeUserOffline(string userId)
        {
            var user = await GetById(userId);

            user.IsOnline = false;

            user.ConnectionId = null;

            await mContext.SaveChangesAsync();
        }

        public async Task<AppUser> GetById(string id)
        {
            return await mUserManager.FindByIdAsync(id);
        }

        public async Task<AppUser> GetByEmail(string email)
        {
            return await mUserManager.FindByEmailAsync(email);
        }

        public async Task<AppUser> GetByUsername(string username)
        {
            return await mUserManager.FindByNameAsync(username);
        }

        public async Task<bool> CheckPassword(string password, AppUser user)
        {
           return await mUserManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> CreateUser(AppUser user, string password)
        {
            return await mUserManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> CreateUser(AppUser user)
        {
            return await mUserManager.CreateAsync(user);
        }

        public async Task<IdentityResult> DeleteUser(AppUser user)
        {
            return await mUserManager.DeleteAsync(user);
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
            await mContext.SaveChangesAsync();
        }

        public async Task ChangeName(string newName, string userId)
        {
            var user = await GetById(userId);
            user.FirstName = newName;
            await mContext.SaveChangesAsync();
        }

        public async Task ChangeLastName(string newName, string userId)
        {
            var user = await GetById(userId);
            user.LastName = newName;
            await mContext.SaveChangesAsync();
        }

        public async Task ChangeUsername(string newName, string userId)
        {
            var user = await GetById(userId);
            user.UserName = newName;
            await mContext.SaveChangesAsync();
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
            await mContext.SaveChangesAsync();
        }

        public async Task UpdateAvatar(string thumbnail, string fullSized, string userId)
        {
            var user = await GetById(userId);
            user.ProfilePicImageURL = thumbnail;
            user.FullImageUrl = fullSized;
            await mContext.SaveChangesAsync();
        }
    }
}
