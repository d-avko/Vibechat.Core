using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.Services;
using Vibechat.Web.Services.Login;
using VibeChat.Web.ApiModels;

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
        public async Task<ResponseApiModel<LoginResultApiModel>> LogInAsync([FromBody]LoginCredentialsApiModel loginCredentials)
        {
            try
            {
                var result = await loginService.LogInAsync(loginCredentials);

                return new ResponseApiModel<LoginResultApiModel>()
                {
                    IsSuccessfull = true,
                    Response = result
                };

            }
            catch (Exception ex)
            {
                return new ResponseApiModel<LoginResultApiModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                };
            }
        }

    }
}
