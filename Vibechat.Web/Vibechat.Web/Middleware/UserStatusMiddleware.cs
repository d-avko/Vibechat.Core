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

        public UserStatusMiddleware(IUsersRepository repository)
        {
            this.repository = repository;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            //update online status for each authorized request.
            if (!context.User.Claims.Count().Equals(0))
            {
                await repository.MakeUserOnline(JwtHelper.GetNamedClaimValue(context.User.Claims));
            }

            await next(context);
        }
    }
}
