using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Vibechat.Web.Services.Users;

namespace Vibechat.Web.Services.Extension_methods
{
    public static class HttpContextExtensions
    {
        private static string[] SpaPaths =
        {
            "/chat",
            "/login",
            "/register"
        };

        private static string[] Locales =
        {
            "/ru",
            "/en"
        };

        private static string Api = "/api";
            
        public static bool IsSpaPath(this HttpContext context)
        {
            var requestPath = context.Request.Path;

            if (requestPath.StartsWithSegments("/sockjs-node") 
                || requestPath.Value.StartsWith("/assets/icons"))
            {
                return true;
            }
            
            if (requestPath.StartsWithSegments(Api))
            {
                return false;
            }
            
            if (!requestPath.HasValue || requestPath.Value.Equals("/") || requestPath.Value.Equals("/index.html"))
            {
                if (requestPath.Value.Equals("/index.html"))
                {
                    context.Request.Path = "/";
                }
                
                return true;
            }

            // /ru/file.js /ru/chat
            foreach (var loc in Locales)
            {
                if (requestPath.StartsWithSegments(loc))
                {
                    return true;
                }
            }

            // /chat, /login ..

            foreach (var path in SpaPaths)
            {
                if (requestPath.StartsWithSegments(path))
                {
                    return true;
                }
            }
            
            return false;
        }

        public static bool IsEnglishRequest(this HttpContext context, IServiceProvider provider)
        {
            var requestPath = context.Request.Path;
            var culture = provider.GetService<UserCultureService>().GetUserCulture(context);
            
            if (requestPath.StartsWithSegments("/en"))
            {
                return true;
            }

            if (Locales.Any(loc => requestPath.StartsWithSegments(loc) && loc != "/en"))
            {
                return false;
            }
            
            if (culture == UserCulture.English)
            {
                return true;
            }

            return false;
        }

        public static bool IsRussianRequest(this HttpContext context, IServiceProvider provider)
        {
            var requestPath = context.Request.Path;
            var culture = provider.GetService<UserCultureService>().GetUserCulture(context);
            
            if (requestPath.StartsWithSegments("/ru"))
            {
                return true;
            }
            
            if (Locales.Any(loc => requestPath.StartsWithSegments(loc) && loc != "/ru"))
            {
                return false;
            }
            
            if (culture == UserCulture.Russian)
            {
                return true;
            }

            return false;
        }

        public static void NormalizeApiCall(this HttpContext context)
        {
            foreach (var path in Locales)
            {
                if (context.Request.Path.StartsWithSegments(path))
                {
                    context.Request.Path = context.Request.Path.ToString().Replace(path, string.Empty);
                }
            }
        }
    }
}