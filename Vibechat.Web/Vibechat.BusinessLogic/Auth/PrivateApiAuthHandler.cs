using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.DataLayer.Repositories;

namespace Vibechat.BusinessLogic.Auth
{
    public class PrivateApiAuthHandler : AuthorizationHandler<PrivateApiRequirement>
    {
        private readonly IUsersRepository usersRepository;

        public PrivateApiAuthHandler(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PrivateApiRequirement requirement)
        {
            string userIdClaim;

            if ((userIdClaim = JwtHelper.GetNamedClaimValue(context.User.Claims)) == null)
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

            if (user.IsAdmin)
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