using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vibechat.Web.ApiModels;
using Vibechat.Web.Services.Login;

namespace VibeChat.Web.Controllers
{
    public class LoginController : Controller
    {
        protected LoginService loginService;

        public LoginController(LoginService loginService)
        {
            this.loginService = loginService;
        }

        [Route("api/login")]
        public async Task<ResponseApiModel<LoginResultApiModel>> LogInAsync(
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