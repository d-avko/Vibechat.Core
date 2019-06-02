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

        private UserManager<UserInApplication> mUserManager { get; set; }

        public UsersRepository(ApplicationDbContext dbContext, UserManager<UserInApplication> mUserManager)
        {
            this.mContext = dbContext;
            this.mUserManager = mUserManager;
        }

        public async Task MakeUserOnline(string userId, string signalRConnectionId)
        {
            UserInApplication user = await GetById(userId);

            user.IsOnline = true;

            user.LastSeen = DateTime.UtcNow;

            user.ConnectionId = signalRConnectionId;

            await mContext.SaveChangesAsync();
        }

        public async Task MakeUserOffline(string userId)
        {
            var user = await GetById(userId);

            user.IsOnline = false;

            user.ConnectionId = null;

            await mContext.SaveChangesAsync();
        }

        public async Task<UserInApplication> GetById(string id)
        {
            return await mUserManager.FindByIdAsync(id);
        }

        public async Task<UserInApplication> GetByEmail(string email)
        {
            return await mUserManager.FindByEmailAsync(email);
        }

        public async Task<UserInApplication> GetByUsername(string username)
        {
            return await mUserManager.FindByNameAsync(username);
        }

        public async Task<bool> CheckPassword(string password, UserInApplication user)
        {
           return await mUserManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> CreateUser(UserInApplication user, string password)
        {
            return await mUserManager.CreateAsync(user, password);
        }

        public async Task<IQueryable<UserInApplication>> FindByUsername(string username)
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

        public async Task UpdateThumbnail(string thumbnail, string fullSized, string userId)
        {
            var user = await GetById(userId);
            user.ProfilePicImageURL = thumbnail;
            user.FullImageUrl = fullSized;
            await mContext.SaveChangesAsync();
        }
    }
}
