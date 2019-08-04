using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VibeChat.Web;
using Vibechat.Web.ApiModels;
using VibeChat.Web.ChatData;
using Vibechat.Web.Data.Repositories;
using Vibechat.Web.Extensions;
using Vibechat.Web.Services.FileSystem;
using static VibeChat.Web.Controllers.UsersController;

namespace Vibechat.Web.Services.Users
{
    public class UsersService
    {
        public const int MaxThumbnailLengthMB = 5;
        public const int MaxNameLength = 128;
        private readonly IContactsRepository contactsRepository;
        private readonly FilesService imagesService;
        private readonly UnitOfWork unitOfWork;
        private readonly IUsersRepository usersRepository;

        public UsersService(
            IUsersRepository usersRepository,
            FilesService imagesService,
            IContactsRepository contactsRepository,
            UnitOfWork unitOfWork)
        {
            this.usersRepository = usersRepository;
            this.imagesService = imagesService;
            this.contactsRepository = contactsRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<UserInfo> GetUserById(UserByIdApiModel userId)
        {
            if (userId == null) throw new FormatException("Provided user was null");

            var FoundUser = await usersRepository.GetById(userId.Id);

            if (FoundUser == null) throw new FormatException("User was not found");

            return FoundUser.ToUserInfo();
        }


        public async Task UpdateUserInfo(string username, string firstname, string lastname, string whoCalled)
        {
            await ChangeUsername(username, whoCalled);
            await ChangeName(firstname, whoCalled);
            await ChangeLastName(lastname, whoCalled);
        }

        public async Task ChangeName(string newName, string whoCalled)
        {
            newName = newName.Replace(" ", "");

            if (newName.Length > MaxNameLength) throw new FormatException("Name was too long.");

            await usersRepository.ChangeName(newName, whoCalled);

            await unitOfWork.Commit();
        }

        public async Task ChangeUsername(string newName, string whoCalled)
        {
            newName = newName.Replace(" ", "");

            if (newName.Length > MaxNameLength) throw new FormatException("Name was too long.");

            var foundUser = await usersRepository.GetByUsername(newName);

            if (foundUser != null) throw new InvalidDataException("New username was not unique.");

            await usersRepository.ChangeUsername(newName, whoCalled);
        }

        public async Task<List<UserInfo>> GetContacts(string callerId)
        {
            var caller = await usersRepository.GetById(callerId);

            if (caller == null) throw new ArgumentException("caller id was wrong.");

            return contactsRepository.GetContactsOf(callerId)
                .Select(x => x.Contact.ToUserInfo())
                ?.ToList();
        }

        public async Task AddToContacts(string userId, string callerId)
        {
            if (userId == callerId) throw new InvalidDataException("Can't add yourself to contacts.");

            try
            {
                contactsRepository.AddContact(callerId, userId);
                await unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Wrong caller id or user id", ex);
            }
        }

        public async Task RemoveFromContacts(string userId, string caller)
        {
            try
            {
                contactsRepository.RemoveContact(caller, userId);
                await unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Wrong caller id or user id", ex);
            }
        }

        public async Task ChangeLastName(string newName, string whoCalled)
        {
            newName = newName.Replace(" ", "");

            if (newName.Length > MaxNameLength) throw new FormatException("Name was too long.");

            await usersRepository.ChangeLastName(newName, whoCalled);

            await unitOfWork.Commit();
        }

        public async Task<AppUser> GetUserById(string userId)
        {
            if (userId == null) throw new FormatException("Provided user was null");

            var FoundUser = await usersRepository.GetById(userId);

            if (FoundUser == null) throw new FormatException("User was not found");

            return FoundUser;
        }

        public async Task<UpdateProfilePictureResponse> UpdateThumbnail(IFormFile image, string userId)
        {
            if (image.Length / (1024 * 1024) > MaxThumbnailLengthMB)
                throw new InvalidDataException($"Thumbnail was larger than {MaxThumbnailLengthMB}");

            var user = usersRepository.GetById(userId);

            if (user == null) throw new FormatException("User was not found");

            ValueTuple<string, string> thumbnailFull;

            try
            {
                using (var buffer = new MemoryStream())
                {
                    image.CopyTo(buffer);
                    buffer.Seek(0, SeekOrigin.Begin);

                    //thumbnail; fullsized
                    thumbnailFull =
                        await imagesService.SaveProfileOrChatPicture(image, buffer, image.FileName, userId, userId);

                    await usersRepository.UpdateAvatar(thumbnailFull.Item1, thumbnailFull.Item2, userId);

                    await unitOfWork.Commit();
                }

                return new UpdateProfilePictureResponse
                {
                    ThumbnailUrl = thumbnailFull.Item1,
                    FullImageUrl = thumbnailFull.Item2
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update profile picture. Try different one.", ex);
            }
        }

        public async Task<UsersByNickNameResultApiModel> FindUsersByNickName(UsersByNickNameApiModel credentials)
        {
            if (credentials.UsernameToFind == null) throw new FormatException("Nickname was null");

            var result = (await usersRepository.FindByUsername(credentials.UsernameToFind)).ToList();

            if (result.Count() == 0)
                return new UsersByNickNameResultApiModel
                {
                    UsersFound = null
                };

            return new UsersByNickNameResultApiModel
            {
                UsersFound = result.Select(FoundUser =>
                    FoundUser.ToUserInfo()
                ).ToList()
            };
        }

        public async Task ChangeUserIsPublicState(string userId, string whoAccessedId)
        {
            if (whoAccessedId != userId) throw new FormatException("Can only call this method for yourself.");

            await usersRepository.ChangeUserPublicState(userId);
            await unitOfWork.Commit();
        }

        public async Task MakeUserOnline(string userId, string signalRConnectionId)
        {
            await usersRepository.MakeUserOnline(userId, signalRConnectionId);
            await unitOfWork.Commit();
        }

        public async Task MakeUserOffline(string userId)
        {
            await usersRepository.MakeUserOffline(userId);
            await unitOfWork.Commit();
        }
    }
}