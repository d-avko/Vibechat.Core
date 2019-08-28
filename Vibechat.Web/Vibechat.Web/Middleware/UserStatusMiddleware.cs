using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VibeChat.Web;
using Vibechat.Web.Data.Repositories;

namespace Vibechat.Web.Middleware
{
    public class UserStatusMiddleware : IMiddleware
    {
        private readonly IUsersRepository repository;
        private readonly UnitOfWork unitOfWork;

        public UserStatusMiddleware(IUsersRepository repository, UnitOfWork unitOfWork)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            //update online status for each authorized request.
            if (context.User.Identity.IsAuthenticated)
            {
                var user = await repository.GetByIdAsync(JwtHelper.GetNamedClaimValue(context.User.Claims));

                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }
                
                await repository.MakeUserOnline(user);
                await repository.UpdateAsync(user);
                await unitOfWork.Commit();
            }

            await next(context);
        }
    }
}