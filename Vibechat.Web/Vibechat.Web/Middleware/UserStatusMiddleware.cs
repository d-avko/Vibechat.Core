using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Services.Repositories;
using VibeChat.Web;

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
            if (!context.User.Claims.Count().Equals(0))
            {
                await repository.MakeUserOnline(JwtHelper.GetNamedClaimValue(context.User.Claims));
                await unitOfWork.Commit();
            }

            await next(context);
        }
    }
}
