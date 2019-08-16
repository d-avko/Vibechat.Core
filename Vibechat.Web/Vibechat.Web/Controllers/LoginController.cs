using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vibechat.Web.ApiModels;
using Vibechat.Web.Services.Login;

namespace VibeChat.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly LoginService loginService;

        public LoginController(LoginService loginService)
        {
            this.loginService = loginService;
        }

        /// <summary>
        /// Signs in user using firebase-issued JWT token (obtained via SMS confirmation).
        /// </summary>
        /// <param name="loginCredentials"></param>
        /// <returns></returns>
        [Route("")]
        [HttpPost]
        public async Task<ResponseApiModel<LoginResultApiModel>> Login(
            [FromBody] LoginCredentialsApiModel loginCredentials)
        {
            try
            {
                var result = await loginService.LogInAsync(loginCredentials);

                return new ResponseApiModel<LoginResultApiModel>
                {
                    IsSuccessfull = true,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<LoginResultApiModel>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}