using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.Extensions;
using Vibechat.Web.Services.FileSystem;
using Vibechat.Web.Services.Repositories;
using VibeChat.Web;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;
using static VibeChat.Web.Controllers.UsersController;

namespace Vibechat.Web.Services.Users
{
    public class UsersInfoService
    {
        private readonly IUsersRepository usersRepository;
        private readonly ImagesService imagesService;
        private readonly IContactsRepository contactsRepository;

        public const int MaxThumbnailLengthMB = 5;
        public const int MaxNameLength = 128;

        public UsersInfoService(
            IUsersRepository usersRepository,
            ImagesService imagesService,
            IContactsRepository contactsRepository)
        {
            this.usersRepository = usersRepository;
            this.imagesService = imagesService;
            this.contactsRepository = contactsRepository;
        }

        public async Task<UserInfo> GetUserById(UserByIdApiModel userId)
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

            return FoundUser.ToUserInfo();
        }

        public async Task ChangeName(string newName, string whoCalled)
        {
            if(newName.Length > MaxNameLength)
            {
                throw new FormatException("Name was too long.");
            }

            await usersRepository.ChangeName(newName, whoCalled);
        }

        public async Task<List<UserInfo>> GetContacts(string callerId)
        {
            UserInApplication caller = await usersRepository.GetById(callerId);

            if (caller == null)
            {
                throw new ArgumentException("caller id was wrong.");
            }

            return contactsRepository.GetContactsOf(callerId)
                .Select(x => x.Contact.ToUserInfo())
                ?.ToList();
        }

        public async Task AddToContacts(string userId, string callerId)
        {
            UserInApplication contact = await usersRepository.GetById(userId);

            if(contact == null)
            {
                throw new ArgumentException("user id was wrong.");
            }

            UserInApplication caller = await usersRepository.GetById(callerId);

            if (caller == null)
            {
                throw new ArgumentException("caller id was wrong.");
            }

            await contactsRepository.AddContact(caller, contact);
        }

        public async Task RemoveFromContacts(string userId, string caller)
        {
            try
            {
                await contactsRepository.RemoveContact(caller, userId);
            }
            catch (Exception ex)
            {                
                throw new InvalidDataException("Wrong caller id or user id", ex);
            }
        }

        public async Task ChangeLastName(string newName, string whoCalled)
        {
            if (newName.Length > MaxNameLength)
            {
                throw new FormatException("Name was too long.");
            }

            await usersRepository.ChangeLastName(newName, whoCalled);
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

        public async Task<UpdateProfilePictureResponse> UpdateThumbnail(IFormFile image, string userId)
        {
            if ((image.Length / (1024 * 1024)) > MaxThumbnailLengthMB)
            {
                throw new InvalidDataException($"Thumbnail was larger than {MaxThumbnailLengthMB}");
            }

            var user = usersRepository.GetById(userId);

            if(user == null)
            {
                throw new FormatException("User was not found");
            }

            Tuple<string, string> thumbnailFull;

            using (var buffer = new MemoryStream())
            {
                image.CopyTo(buffer);
                buffer.Seek(0, SeekOrigin.Begin);

                //thumbnail; fullsized
               thumbnailFull = imagesService.SaveImage(buffer, image.FileName);

                await usersRepository.UpdateThumbnail(thumbnailFull.Item1, thumbnailFull.Item2, userId);
            }

            return new UpdateProfilePictureResponse()
            {
                ThumbnailUrl = thumbnailFull.Item1,
                FullImageUrl = thumbnailFull.Item2
            };
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
                return new UsersByNickNameResultApiModel()
                {
                    UsersFound = null
                };
            }

            return new UsersByNickNameResultApiModel()
            {
                UsersFound = result.Select((FoundUser) => 
                FoundUser.ToUserInfo()
                ).ToList()
            };
        }

        public async Task ChangeUserIsPublicState(string userId, string whoAccessedId)
        {
            if(whoAccessedId != userId)
            {
                throw new FormatException("Can only call this method for yourself.");
            }

            await usersRepository.ChangeUserPublicState(userId);
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
