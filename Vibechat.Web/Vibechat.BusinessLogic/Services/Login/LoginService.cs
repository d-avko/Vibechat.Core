﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.BusinessLogic.Extensions;
using Vibechat.BusinessLogic.Services.ChatDataProviders;
using Vibechat.DataLayer;
using Vibechat.DataLayer.Repositories;
using Vibechat.Shared.ApiModels.Login;
using Vibechat.Shared.ApiModels.Register;

namespace Vibechat.BusinessLogic.Services.Login
{
    public class LoginService
    {
        private readonly UnitOfWork unitOfWork;
        private readonly IJwtTokenGenerator tokenGenerator;

        private readonly IChatDataProvider chatDataProvider;

        private readonly IUsersRepository usersRepository;

        public LoginService(
            IUsersRepository usersRepository,
            IChatDataProvider chatDataProvider,
            UnitOfWork unitOfWork,
            IJwtTokenGenerator tokenGenerator)
        {
            this.usersRepository = usersRepository;
            this.chatDataProvider = chatDataProvider;
            this.unitOfWork = unitOfWork;
            this.tokenGenerator = tokenGenerator;
        }

        public async Task<LoginResultApiModel> LogInAsync(string firebaseToken, string phoneNumber)
        {
            var auth = FirebaseAuth.GetAuth(FirebaseApp.DefaultInstance);
            
            //this can throw detailed error message.
            var verified = await auth.VerifyIdTokenAsync(firebaseToken);

            var identityUser = await usersRepository.GetByIdAsync(verified.Uid);

            //user confirmed his phone number, but has not registered yet; 
            //Register him now in that case 

            var isNewUser = false;

            if (identityUser == null)
            {
                try
                {
                    var username = "user_" + Guid.NewGuid();

                    identityUser = await RegisterNewUserAsync(new RegisterModel
                    {
                        PhoneNumber = phoneNumber,
                        UserName = username,
                        Id = verified.Uid
                    });

                    isNewUser = true;
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("Couldn't register this user.", ex);
                }
            }
            else
            {
                if(identityUser.PhoneNumber != phoneNumber)
                {
                    throw new UnauthorizedAccessException("Please provide a phone number that correlates to firebase JWT token.");
                }
            }

            return new LoginResultApiModel
            {
                Info = identityUser.ToAppUserDto(),
                Token = tokenGenerator.GenerateToken(identityUser),
                RefreshToken = identityUser.RefreshToken,
                IsNewUser = isNewUser
            };
        }

        /// <summary>
        ///     Registers user and issues a refresh token.
        /// </summary>
        /// <param name="userToRegister"></param>
        /// <returns></returns>
        private async Task<AppUser> RegisterNewUserAsync(RegisterModel userToRegister)
        {
            var defaultError = new InvalidDataException("Check the fields and try again.");

            if (userToRegister == null)
            {
                throw defaultError;
            }

            if (string.IsNullOrWhiteSpace(userToRegister.UserName))
            {
                throw defaultError;
            }

            // if UserName and email is not unique

            if (await usersRepository.GetByUsername(userToRegister.UserName) != null)
            {
                throw new InvalidDataException("The username is not unique.");
            }

            var imageUrl = chatDataProvider.GetProfilePictureUrl();

            var userToCreate = new AppUser
            {
                Id = userToRegister.Id,
                UserName = userToRegister.UserName,
                FirstName = userToRegister.FirstName,
                LastName = userToRegister.LastName,
                PhoneNumber = userToRegister.PhoneNumber,
                ProfilePicImageURL = imageUrl,
                FullImageUrl = imageUrl,
                IsPublic = true
            };

            var result = await usersRepository.CreateUser(userToCreate);

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors?.ToList()[0].Description ??
                                          "Couldn't create user because of unexpected error.");
            }

            var token = tokenGenerator.GenerateToken(userToCreate);

            await usersRepository.UpdateRefreshToken(userToCreate, token);
            await usersRepository.UpdateAsync(userToCreate);
            await unitOfWork.Commit();
            return userToCreate;    
        }
    }
}