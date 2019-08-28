﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        public UsersRepository(UserManager<AppUser> mUserManager)
        {
            this.mUserManager = mUserManager;
        }
 
        private readonly UserManager<AppUser> mUserManager;

        public async Task MakeUserOnline(AppUser user, bool updateConnectionId = false, string signalRConnectionId = null)
        {
            user.IsOnline = true;
            user.LastSeen = DateTime.UtcNow;
            
            if (updateConnectionId)
            {
                user.ConnectionId = signalRConnectionId;   
            }
        }

        public async Task MakeUserOffline(AppUser user)
        {
            user.IsOnline = false;

            user.ConnectionId = null;
        }

        public async Task<AppUser> GetByIdAsync(string id)
        {
            return await mUserManager.Users
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task LockoutUser(AppUser user, DateTimeOffset until)
        {
            return mUserManager.SetLockoutEndDateAsync(user, until);
        }

        public Task DisableUserLockout(AppUser user)
        {
            return mUserManager.SetLockoutEndDateAsync(user, DateTimeOffset.MinValue);
        }

        public Task<AppUser> GetByUsername(string username)
        {
            return mUserManager.FindByNameAsync(username);
        }

        public Task<IdentityResult> CreateUser(AppUser user)
        {
            return mUserManager.CreateAsync(user);
        }

        public async Task<IQueryable<AppUser>> FindByUsername(string username)
        {
            return mUserManager
                .Users
                .Where(user => user.IsPublic)
                .Where(user => EF.Functions.Like(user.UserName.ToLower(), username.ToLower() + "%"));
        }

        public Task UpdateAsync(AppUser user)
        {
            return mUserManager.UpdateAsync(user);
        }

        public async Task ChangeUserPublicState(AppUser user)
        {
            user.IsPublic = !user.IsPublic;
        }

        public async Task ChangeName(string newName, AppUser user)
        {
            user.FirstName = newName;
        }

        public async Task ChangeLastName(string newName, AppUser user)
        {
            user.LastName = newName;
        }

        public async Task ChangeUsername(string newName, AppUser user)
        {
            user.UserName = newName;
        }

        public async Task UpdateRefreshToken(AppUser user, string token)
        {
            user.RefreshToken = token;
        }

        public async Task UpdateAvatar(string thumbnail, string fullSized, AppUser user)
        {
            user.ProfilePicImageURL = thumbnail;
            user.FullImageUrl = fullSized;
        }
    }
}