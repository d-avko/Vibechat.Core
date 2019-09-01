using Microsoft.AspNetCore.Identity;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.DataLayer;

namespace Vibechat.BusinessLogic.Extensions
{
    public static class UserManagerExtensions
    {
        public static void SeedAdminAccount(this UserManager<AppUser> value, IJwtTokenGenerator tokenGenerator)
        {
            var admin = new AppUser()
            {
                IsAdmin = true,
                UserName = "admin"
            };
            
            admin.RefreshToken = tokenGenerator.GenerateRefreshToken(admin);
            //if account is created, usermanager won't do anything.
            value.CreateAsync(admin).GetAwaiter().GetResult();
        }
    }
}