using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VibeChat.Web;
using Vibechat.Web.ApiModels;
using Vibechat.Web.AuthHelpers;
using Vibechat.Web.Services.Users;

namespace Vibechat.Web.Controllers
{
    public class TokensController : Controller
    {
        public TokensController(ITokenValidator tokensValidator, UsersService userService)
        {
            this.tokensValidator = tokensValidator;
            this.userService = userService;
        }

        protected ITokenValidator tokensValidator { get; set; }

        protected UsersService userService { get; set; }

        [Route("api/Tokens/Refresh")]
        public async Task<ResponseApiModel<string>> RefreshToken([FromBody] RefreshTokenRequest tokenInfo)
        {
            var defaultError = new ResponseApiModel<string>
            {
                IsSuccessfull = false,
                ErrorMessage = "Either refresh token expired or wrong credentials were provided."
            };

            if (tokenInfo.RefreshToken == null || tokenInfo.userId == null)
            {
                return defaultError;
            }

            if (!await tokensValidator.Validate(tokenInfo.userId, tokenInfo.RefreshToken))
            {
                return defaultError;
            }

            var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);
            
            return new ResponseApiModel<string>
            {
                IsSuccessfull = true,
                Response = (await userService.GetUserById(tokenInfo.userId)).GenerateToken()
            };
        }

        public class RefreshTokenRequest
        {
            public string RefreshToken { get; set; }

            public string userId { get; set; }
        }
    }
}