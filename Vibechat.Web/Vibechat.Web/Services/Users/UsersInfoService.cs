using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.Services.Repositories;
using VibeChat.Web;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;

namespace Vibechat.Web.Services.Users
{
    public class UsersInfoService
    {
        private readonly IUsersRepository usersRepository;

        public UsersInfoService(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository;
        }

        public async Task<UserByIdApiResponseModel> GetUserById(UserByIdApiModel userId)
        {
            if (userId == null)
            {
                throw new FormatException("Provided user was null");
            }

            var FoundUser = await usersRepository.GetById(userId.Id);

            if (FoundUser == null)
            {
                throw new FormatException("User was not found");
            }


            return new UserByIdApiResponseModel()
            {
                User = new UserInfo()
                {
                    Id = FoundUser.Id,
                    ImageUrl = FoundUser.ProfilePicImageURL,
                    LastName = FoundUser.LastName,
                    LastSeen = FoundUser.LastSeen,
                    Name = FoundUser.FirstName,
                    UserName = FoundUser.UserName,
                    ConnectionId = FoundUser.ConnectionId,
                    IsOnline = FoundUser.IsOnline
                }
            };

        }

        public async Task<UserInApplication> GetUserById(string userId)
        {
            if (userId == null)
            {
                throw new FormatException("Provided user was null");
            }

            var FoundUser = await usersRepository.GetById(userId);

            if (FoundUser == null)
            {
                throw new FormatException("User was not found");
            }

            return FoundUser;
        }

        public async Task<UsersByNickNameResultApiModel> FindUsersByNickName(UsersByNickNameApiModel credentials)
        {
            if (credentials.UsernameToFind == null)
            {
                throw new FormatException("Nickname was null");
            }

            var result = (await usersRepository.FindByUsername(credentials.UsernameToFind)).ToList();

            if (result.Count() == 0)
            {
                throw new FormatException("Noone was found.");
            }

            return new UsersByNickNameResultApiModel()
            {
                UsersFound = result.Select((FoundUser) => new FoundUser
                {
                    ID = FoundUser.Id,
                    Username = FoundUser.UserName,
                    FirstName = FoundUser.FirstName,
                    LastName = FoundUser.LastName
                }).ToList()
            };
        }

        public async Task MakeUserOnline(string userId, string signalRConnectionId)
        {
            await usersRepository.MakeUserOnline(userId, signalRConnectionId);
        }

        public async Task MakeUserOffline(string userId)
        {
            await usersRepository.MakeUserOffline(userId);
        }

    }
}
