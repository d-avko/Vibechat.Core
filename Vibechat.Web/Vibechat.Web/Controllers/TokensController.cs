using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.AuthHelpers;
using Vibechat.Web.Data.ApiModels.Tokens;
using Vibechat.Web.Services;
using Vibechat.Web.Services.Users;
using VibeChat.Web;

namespace Vibechat.Web.Controllers
{
    public class TokensController : Controller
    {
        protected ITokenClaimValidator tokensValidator { get; set; }

        protected UsersInfoService userService { get; set; }

        public TokensController(ITokenClaimValidator tokensValidator, UsersInfoService userService)
        {
            this.tokensValidator = tokensValidator;
            this.userService = userService;
        }

        [Route("api/Tokens/Refresh")]
        public async Task<ResponseApiModel<RefreshTokenResponse>> RefreshToken([FromBody]RefreshTokenApiModel tokenInfo)
        {
            var defaultError = new ResponseApiModel<RefreshTokenResponse>()
            {
                IsSuccessfull = false,
                ErrorMessage = "Wrong user id or token were provided.",
            };

            if(tokenInfo.OldToken == null || tokenInfo.UserId == null)
            {
                return defaultError;
            }

            if (!tokensValidator.Validate(tokenInfo.OldToken, JwtHelper.JwtUserIdClaimName, tokenInfo.UserId))
            {
                return defaultError;
            }

            return new ResponseApiModel<RefreshTokenResponse>()
            {
                IsSuccessfull = true,
                Response = new RefreshTokenResponse()
                {
                    Token = JwtHelper.GenerateJwtToken(await userService.GetUserById(tokenInfo.UserId))
                }
            };
        }
    }
}
