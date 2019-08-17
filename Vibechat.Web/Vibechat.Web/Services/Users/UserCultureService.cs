using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System;
using System.Linq;

namespace Vibechat.Web.Services.Users
{
    public enum UserCulture
    {
        English,
        Russian
    }
    
    public class UserCultureService
    {
        public UserCulture GetUserCulture(HttpContext context)
        {
            var priorityLanguage = (context.Request.GetTypedHeaders()
            .AcceptLanguage?
            .OrderByDescending(x => x.Quality ?? 1)
            .Select(x => x.Value.ToString())
            .ToArray() ?? Array.Empty<string>()).FirstOrDefault();

            if (priorityLanguage == null)
            {
                return UserCulture.English;
            }
            
            switch (priorityLanguage)
            {
                case string lang when lang.StartsWith("ru"):
                case string byLang when byLang.StartsWith("be"):
                {
                    return UserCulture.Russian;
                }
                default:
                {
                    return UserCulture.English;
                }
            }
        }
    }
}