using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using VibeChat.Web.ApiModels;

namespace VibeChat.Web.Controllers
{
    public class LoginController : Controller
    {
        protected UserManager<UserInApplication> mUserManager;

        public LoginController(UserManager<UserInApplication> userManager)
        {
            mUserManager = userManager;
        }

        [Route("api/login")]
        public async Task<ResponseApiModel<LoginResultApiModel>> LogInAsync([FromBody]LoginCredentialsApiModel loginCredentials)
        {
            var defaultError = new ResponseApiModel<LoginResultApiModel>()
            {
                ErrorMessage = "Wrong username or password",
                IsSuccessfull = false
            };

            if ((loginCredentials?.UserNameOrEmail == null) || (string.IsNullOrWhiteSpace(loginCredentials.UserNameOrEmail)))
            {
                //if login failed
                return defaultError;
            }

            UserInApplication user;

            if (loginCredentials.UserNameOrEmail.Contains("@"))
            {
                user = await mUserManager.FindByEmailAsync(loginCredentials.UserNameOrEmail);
            }
            else
            {
                user = await mUserManager.FindByNameAsync(loginCredentials.UserNameOrEmail);
            }

            if (user == null)
            {
                return defaultError;
            }

           if(!await mUserManager.CheckPasswordAsync(user, loginCredentials.Password).ConfigureAwait(false))
           {
                return defaultError;
           }

            //if we are here then have valid password and login of a user

            return new ResponseApiModel<LoginResultApiModel>()
            {
                // return token and user info
                IsSuccessfull = true,

                Response = new LoginResultApiModel
                {
                    Firstname = user.FirstName,
                    LastSeen = user.LastSeen,
                    ProfilePicImageURL = user.ProfilePicImageURL,
                    ProfilePicRgb = user.ProfilePicRgb,
                    Lastname = user.LastName,
                    Email = user.Email,
                    UserName = user.UserName,
                    Token = user.GenerateJwtToken(),
                    ID = user.Id,
                    ImageUrl = user.ProfilePicImageURL,
                }

            };
        }

        [Route("api/Register")]
        public async Task<ResponseApiModel<string>> RegisterNewUserAsync([FromBody]RegisterInformationApiModel userToRegister)
        {
            var defaultError = new ResponseApiModel<string>()
            {
                ErrorMessage = "Check the fields and try again.",
                IsSuccessfull = false
            };

            var EmailFormatError = new ResponseApiModel<string>()
            {
                ErrorMessage = "Email is in wrong format!",
                IsSuccessfull = false
            };


            if (userToRegister == null)
                return defaultError;

            if (string.IsNullOrWhiteSpace(userToRegister.Email) || string.IsNullOrWhiteSpace(userToRegister.Password) || string.IsNullOrWhiteSpace(userToRegister.UserName))
            {
                return defaultError;
            }

            if (userToRegister.UserName.Contains("@") || userToRegister.UserName.Contains("@"))
            {
                return new ResponseApiModel<string>()
                {
                    ErrorMessage = "Nickname or Username cannot contain '@'",
                    IsSuccessfull = false
                };
            }

            if (!Regex.Match(userToRegister.Email, "[^@]*@[^\\.]\\.(\\w+)").Success)
            {
                return EmailFormatError;
            }

            // if UserName and email are not unique

            if ((mUserManager.FindByNameAsync(userToRegister.UserName) == null) && (mUserManager.FindByEmailAsync(userToRegister.Email) == null))
            {
                return new ResponseApiModel<string>()
                {
                    ErrorMessage = "The username or e-mail is not unique.",
                    IsSuccessfull = false
                };
            }

            var userToCreate = new UserInApplication()
            {
                UserName = userToRegister.UserName,
                Email = userToRegister.Email,
                FirstName = userToRegister.FirstName,
                LastName = userToRegister.LastName,
                ProfilePicRgb = BackgroundColors.GetProfilePicRgb(),
            };

            
            var result = await mUserManager.CreateAsync(userToCreate, userToRegister.Password);

            if (!result.Succeeded)
            {
                return new ResponseApiModel<string>
                {
                    //return first error description
                    ErrorMessage = result.Errors?.ToList()[0].Description,
                    IsSuccessfull = false
                };
            }

            
            return new ResponseApiModel<string>()
            {
                IsSuccessfull = true,
                Response = null
            };


        }
    }
}
