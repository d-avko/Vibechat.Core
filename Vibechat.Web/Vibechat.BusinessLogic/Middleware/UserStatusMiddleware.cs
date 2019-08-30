using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.DataLayer.Repositories;

namespace Vibechat.BusinessLogic.Middleware
{
    public class UserStatusMiddleware : IMiddleware
    {
        private readonly IUsersRepository repository;
        private readonly UnitOfWork unitOfWork;
        private readonly ILogger<UserStatusMiddleware> logger;

        public UserStatusMiddleware(IUsersRepository repository, UnitOfWork unitOfWork, 
            ILogger<UserStatusMiddleware> logger)
        {
            this.repository = repository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
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

                try
                {
                    await repository.MakeUserOnline(user);
                    await repository.UpdateAsync(user);
                    await unitOfWork.Commit();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    //could happen if signalR event and api call came in at the same time.
                    logger.LogWarning("Error while updating user 'online' state, exception:" + e.Message);
                }
            }

            await next(context);
        }
    }
}