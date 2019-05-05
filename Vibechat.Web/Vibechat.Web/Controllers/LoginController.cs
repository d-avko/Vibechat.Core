using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.Services;
using VibeChat.Web.ApiModels;

namespace VibeChat.Web.Controllers
{
    public class LoginController : Controller
    {
        protected DatabaseService mDbService;

        public LoginController(DatabaseService mDbService)
        {
            this.mDbService = mDbService;
        }

        [Route("api/login")]
        public async Task<ResponseApiModel<LoginResultApiModel>> LogInAsync([FromBody]LoginCredentialsApiModel loginCredentials)
        {
            try
            {
                var result = await mDbService.LogInAsync(loginCredentials);

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

        [Route("api/Register")]
        public async Task<ResponseApiModel<string>> RegisterNewUserAsync([FromBody]RegisterInformationApiModel userToRegister)
        {
            try
            {
                await mDbService.RegisterNewUserAsync(userToRegister);

                return new ResponseApiModel<string>()
                {
                    IsSuccessfull = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<string>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                };
            }
        }
    }
}
