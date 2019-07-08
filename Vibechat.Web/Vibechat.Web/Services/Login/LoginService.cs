using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.Extensions;
using Vibechat.Web.Services.ChatDataProviders;
using Vibechat.Web.Services.Repositories;
using VibeChat.Web;
using VibeChat.Web.ApiModels;

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
            FirebaseAuth auth = FirebaseAuth.GetAuth(FirebaseApp.DefaultInstance);

            //this can throw detailed error message.
            FirebaseToken verified = await auth.VerifyIdTokenAsync(loginCredentials.UidToken);

            AppUser identityUser = await usersRepository.GetById(verified.Uid);

            //user confirmed his phone number, but has not registered yet; 
            //Register him now in that case 

            bool IsNewUser = false;

            if(identityUser == null)
            {
                try
                {
                    string username = "Generated_" + Guid.NewGuid().ToString();

                    var token = await RegisterNewUserAsync(new RegisterInformationApiModel()
                    {
                        PhoneNumber = loginCredentials.PhoneNumber,
                        UserName = username,
                        Id = verified.Uid
                    });

                    identityUser = await usersRepository.GetByUsername(username);

                    //prevent reading null
                    identityUser.RefreshToken = token;
                    IsNewUser = true;
                }
                catch (Exception ex)
                {
                    throw new FormatException("Couldn't register this user.", ex);
                }
            }

            return new LoginResultApiModel()
            {
                Info = identityUser.ToUserInfo(),
                Token = identityUser.GenerateToken(),
                RefreshToken = identityUser.RefreshToken,
                IsNewUser = IsNewUser
            };
        }

        /// <summary>
        /// Registers user and issues a refresh token.
        /// </summary>
        /// <param name="userToRegister"></param>
        /// <returns></returns>
        private async Task<string> RegisterNewUserAsync(RegisterInformationApiModel userToRegister)
        {
            var defaultError = new FormatException("Check the fields and try again.");

            if (userToRegister == null)
            {
                throw defaultError;
            }

            if (string.IsNullOrWhiteSpace(userToRegister.UserName))
            {
                throw defaultError;
            }

            // if UserName and email is not unique

            if ((await usersRepository.GetByUsername(userToRegister.UserName)) != null)
            {
                throw new FormatException("The username is not unique.");
            }

            var userToCreate = new AppUser()
            {
                Id = userToRegister.Id,
                UserName = userToRegister.UserName,
                FirstName = userToRegister.FirstName,
                LastName = userToRegister.LastName,
                ProfilePicImageURL = chatDataProvider.GetProfilePictureUrl(),
                FullImageUrl = chatDataProvider.GetProfilePictureUrl(),
                IsPublic = true
            };

            var result = await usersRepository.CreateUser(userToCreate);

            if (!result.Succeeded)
            {
                throw new FormatException(result.Errors?.ToList()[0].Description ?? "Couldn't create user because of unexpected error.");
            }

            string token = userToCreate.GenerateRefreshToken();

            await usersRepository.UpdateRefreshToken(userToCreate.Id, token);

            return token;
        }
    }
}
