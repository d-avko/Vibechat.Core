using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.Extensions;
using Vibechat.Web.Services.ChatDataProviders;
using Vibechat.Web.Services.Repositories;
using VibeChat.Web;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;

namespace Vibechat.Web.Services.Login
{
    public class LoginService
    {
        public LoginService(
            IUsersRepository usersRepository,
            IChatDataProvider chatDataProvider)
        {
            this.usersRepository = usersRepository;
            this.chatDataProvider = chatDataProvider;
        }

        private IUsersRepository usersRepository;

        private IChatDataProvider chatDataProvider;

        public async Task<LoginResultApiModel> LogInAsync(LoginCredentialsApiModel loginCredentials)
        {
            var defaultError = new FormatException("Wrong username or password");

            if ((loginCredentials?.UserNameOrEmail == null) || (string.IsNullOrWhiteSpace(loginCredentials.UserNameOrEmail)))
            {
                throw defaultError;
            }

            UserInApplication user;

            if (Regex.Match(loginCredentials.UserNameOrEmail, "[^@]*@[^\\.]\\.(\\w+)").Success)
            {
                user = await usersRepository.GetByEmail(loginCredentials.UserNameOrEmail);
            }
            else
            {
                user = await usersRepository.GetByUsername(loginCredentials.UserNameOrEmail);
            }

            if (user == null)
            {
                throw defaultError;
            }

            if (!await usersRepository.CheckPassword(loginCredentials.Password, user))
            {
                throw defaultError;
            }

            //if we are here then have valid password and login of a user

            return new LoginResultApiModel()
            {
                Info = user.ToUserInfo(),
                Token = user.GenerateJwtToken(),
            };
        }

        public async Task RegisterNewUserAsync(RegisterInformationApiModel userToRegister)
        {
            var defaultError = new FormatException("Check the fields and try again.");

            var EmailFormatError = new FormatException("Email is in wrong format!");

            if (userToRegister == null)
                throw defaultError;

            if (string.IsNullOrWhiteSpace(userToRegister.Email)
                || string.IsNullOrWhiteSpace(userToRegister.Password)
                || string.IsNullOrWhiteSpace(userToRegister.UserName))
            {
                throw defaultError;
            }

            if (userToRegister.UserName.Contains("@") || userToRegister.UserName.Contains("@"))
            {
                throw new FormatException("Nickname or Username cannot contain '@'");
            }

            if (!Regex.Match(userToRegister.Email, "[^@]*@[^\\.]\\.(\\w+)").Success)
            {
                throw EmailFormatError;
            }

            // if UserName and email are not unique

            if (((await usersRepository.GetByUsername(userToRegister.UserName)) != null) || ((await usersRepository.GetByEmail(userToRegister.Email)) != null))
            {
                throw new FormatException("The username or e-mail is not unique.");
            }

            var userToCreate = new UserInApplication()
            {
                UserName = userToRegister.UserName,
                Email = userToRegister.Email,
                FirstName = userToRegister.FirstName,
                LastName = userToRegister.LastName,
                ProfilePicImageURL = chatDataProvider.GetProfilePictureUrl(),
                IsPublic = true
            };


            var result = await usersRepository.CreateUser(userToCreate, userToRegister.Password);

            if (!result.Succeeded)
            {
                throw new FormatException(result.Errors?.ToList()[0].Description);
            }
        }
    }
}
