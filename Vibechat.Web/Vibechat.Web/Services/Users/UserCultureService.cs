using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

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
            var langName = context.Features.Get<IRequestCultureFeature>()?.RequestCulture?.Culture?
                .TwoLetterISOLanguageName;

            if (langName == null)
            {
                return UserCulture.English;
            }
            
            switch (langName)
            {
                case "ru":
                case "be":
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