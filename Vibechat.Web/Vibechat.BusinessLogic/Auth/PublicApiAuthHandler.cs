using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.DataLayer.Repositories;

namespace Vibechat.BusinessLogic.Auth
{
    public class PublicApiAuthHandler : AuthorizationHandler<PublicApiRequirement>
    {
        private readonly IUsersRepository usersRepository;

        public PublicApiAuthHandler(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository;
        }
        
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PublicApiRequirement requirement)
        {
            string userIdClaim;

            if ((userIdClaim = ClaimsExtractor.GetUserIdClaim(context.User.Claims)) == null)
            {
                context.Fail();
                return;
            }

            var user = await usersRepository.GetByIdAsync(userIdClaim);

            if (user == null)
            {
                context.Fail();
                return;
            }
            
            if (user.LockoutEnd == null || user.LockoutEnd <= DateTime.UtcNow)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}