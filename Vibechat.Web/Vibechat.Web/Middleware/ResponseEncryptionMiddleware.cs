using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Vibechat.Web.Data.ApiModels.misc;
using Vibechat.Web.Services.Users;
using VibeChat.Web;

namespace Vibechat.Web.Middleware
{
    public class ResponseEncryptionMiddleware : IMiddleware
    {
        private readonly SessionsService sessionsService;

        public ResponseEncryptionMiddleware(SessionsService sessionsService)
        {
            this.sessionsService = sessionsService;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                using (var reader = new StreamReader(context.Request.Body))
                {
                    EncryptedRequest result = JObject.Parse(await reader.ReadToEndAsync()).ToObject<EncryptedRequest>();

                    string encryptionKey = await sessionsService.GetAuthKey(result.AuthKeyId, JwtHelper.GetNamedClaimValue(context.User.Claims));


                }
            }
            finally { await next(context); }
        }
    }
}
