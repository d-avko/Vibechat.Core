using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.BusinessLogic.Services.Users;
using Vibechat.Shared.ApiModels;

namespace Vibechat.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        private readonly IJwtTokenGenerator tokenGenerator;

        public TokensController(ITokenValidator tokensValidator, UsersService userService, IJwtTokenGenerator tokenGenerator)
        {
            this.tokenGenerator = tokenGenerator;
            this.tokensValidator = tokensValidator;
            this.userService = userService;
        }

        protected ITokenValidator tokensValidator { get; set; }

        protected UsersService userService { get; set; }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest tokenInfo)
        {
            if (tokenInfo == null)
            {
                return BadRequest();
            }

            if (tokenInfo.RefreshToken == null || tokenInfo.userId == null)
            {
                return BadRequest();
            }

            if (!await tokensValidator.Validate(tokenInfo.userId, tokenInfo.RefreshToken))
            {
                return BadRequest("Wrong refresh token.");
            }

            var user = await userService.GetUserById(tokenInfo.userId);

            return Ok(new ResponseApiModel<string>
            {
                IsSuccessfull = true,
                Response = tokenGenerator.GenerateToken(user)
            });
        }

        public class RefreshTokenRequest
        {
            public string RefreshToken { get; set; }

            public string userId { get; set; }
        }
    }
}