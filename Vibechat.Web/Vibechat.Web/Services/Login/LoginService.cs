using FirebaseAdmin;
using FirebaseAdmin.Auth;
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
            var auth = FirebaseAuth.GetAuth(FirebaseApp.DefaultInstance);

            //this can throw detailed error message.
            FirebaseToken verified = await auth.VerifyIdTokenAsync(loginCredentials.UidToken);

            AppUser identityUser = await usersRepository.GetById(verified.Uid);

            //user confirmed his phone number, but has not registered yet; 
            //Register him now in that case 

            if(identityUser == null)
            {
                try
                {
                    string username = "Generated_" + Guid.NewGuid().ToString();

                    await RegisterNewUserAsync(new RegisterInformationApiModel()
                    {
                        PhoneNumber = loginCredentials.PhoneNumber,
                        UserName = username
                    }, true);

                    identityUser = await usersRepository.GetByUsername(username);
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
                RefreshToken = identityUser.RefreshToken
            };
        }


        public async Task RegisterNewUserAsync(RegisterInformationApiModel userToRegister, bool firebaseUserCreated)
        {
            var auth = FirebaseAuth.GetAuth(FirebaseApp.DefaultInstance);

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
                UserName = userToRegister.UserName,
                FirstName = userToRegister.FirstName,
                LastName = userToRegister.LastName,
                ProfilePicImageURL = chatDataProvider.GetProfilePictureUrl(),
                IsPublic = true
            };

            var result = await usersRepository.CreateUser(userToCreate);

            if (!result.Succeeded)
            {
                throw new FormatException(result.Errors?.ToList()[0].Description ?? "Couldn't create user because of unexpected error.");
            }

            if (!firebaseUserCreated)
            {
                try
                {
                    await auth.CreateUserAsync(new UserRecordArgs()
                    {
                        Uid = userToCreate.Id,
                        PhoneNumber = userToRegister.PhoneNumber
                    });
                }
                catch (Exception ex)
                {
                    //failed to create new user; probably wrong phone number.
                    await usersRepository.DeleteUser(userToCreate);

                    throw new FormatException("Wrong phone number was provided.", ex);
                }
            }

            await usersRepository.UpdateRefreshToken(userToCreate.Id, userToCreate.GenerateRefreshToken());
        }
    }
}
