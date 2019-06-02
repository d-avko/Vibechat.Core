using Microsoft.AspNetCore.Authorization;
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
        protected ITokenValidator tokensValidator { get; set; }

        protected UsersInfoService userService { get; set; }

        public TokensController(ITokenValidator tokensValidator, UsersInfoService userService)
        {
            this.tokensValidator = tokensValidator;
            this.userService = userService;
        }

        public class RefreshTokenRequest
        {
            public string RefreshToken { get; set; }

            public string userId { get; set; }
        }

        [Route("api/Tokens/Refresh")]
        public async Task<ResponseApiModel<string>> RefreshToken([FromBody]RefreshTokenRequest tokenInfo)
        {
            var defaultError = new ResponseApiModel<string>()
            {
                IsSuccessfull = false,
                ErrorMessage = "Either refresh token expired or wrong credentials were provided.",
            };

            if(tokenInfo.RefreshToken == null || tokenInfo.userId == null)
            {
                return defaultError;
            }

            if (!await tokensValidator.Validate(tokenInfo.userId, tokenInfo.RefreshToken))
            {
                return defaultError;
            }

            return new ResponseApiModel<string>()
            {
                IsSuccessfull = true,
                Response = (await userService.GetUserById(tokenInfo.userId)).GenerateToken()
            };
        }
    }
}
