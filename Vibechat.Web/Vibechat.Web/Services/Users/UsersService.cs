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
using Vibechat.Web.Services.Bans;
using Vibechat.Web.Services.FileSystem;
using Vibechat.Web.Data_Layer.Repositories.Specifications.Contacts;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Services.Users
{
    public class UsersService
    {
        public const int MaxThumbnailLengthMB = 5;
        public const int MaxNameLength = 128;
        public const int MinNameLength = 5;
        private readonly IContactsRepository contactsRepository;
        private readonly FilesService imagesService;
        private readonly UnitOfWork unitOfWork;
        private readonly BansService bansService;
        private readonly IUsersRepository usersRepository;

        public UsersService(
            IUsersRepository usersRepository,
            FilesService imagesService,
            IContactsRepository contactsRepository,
            UnitOfWork unitOfWork,
            BansService bansService)
        {
            this.usersRepository = usersRepository;
            this.imagesService = imagesService;
            this.contactsRepository = contactsRepository;
            this.unitOfWork = unitOfWork;
            this.bansService = bansService;
        }

        public async Task<AppUserDto> GetUserById(string userId, string callerId)
        {
            if (userId == null)
            {
                throw new InvalidDataException("Provided user was null");
            }

            var foundUser = await usersRepository.GetByIdAsync(userId);
 
            if (foundUser == null)
            {
                throw new KeyNotFoundException("User was not found");
            }

            var user = foundUser.ToAppUserDto();
            
            user.IsMessagingRestricted =
                bansService.IsBannedFromMessagingWith(callerId, userId);

            user.IsBlocked =
                bansService.IsBannedFromMessagingWith(userId, callerId);

            return user;
        }
        
        public async Task<AppUser> GetUserById(string userId)
        {
            if (userId == null)
            {
                throw new InvalidDataException("Provided user was null");
            }

            var foundUser = await usersRepository.GetByIdAsync(userId);
 
            if (foundUser == null)
            {
                throw new InvalidDataException("User was not found");
            }

            return foundUser;
        }


        public async Task UpdateUserInfo(string username, string firstname, string lastname, string whoCalled)
        {
            await ChangeUsername(username, whoCalled);
            await ChangeName(firstname, whoCalled);
            await ChangeLastName(lastname, whoCalled);
        }

        public async Task ChangeName(string newName, string whoCalled)
        {
            if (newName == null)
            {
                throw new InvalidDataException("New name cannot be null");
            }
            
            newName = newName.Replace(" ", "");

            if (newName.Length > MaxNameLength)
            {
                throw new InvalidDataException("Name was too long.");
            }
            
            var user = await usersRepository.GetByIdAsync(whoCalled);

            if (user == null)
            {
                throw new KeyNotFoundException("");
            }
            
            await usersRepository.ChangeName(newName, user);
            await usersRepository.UpdateAsync(user);
            await unitOfWork.Commit();
        }
        
        public async Task ChangeLastName(string newName, string whoCalled)
        {
            if (newName == null)
            {
                throw new InvalidDataException("New name cannot be null");
            }
            
            newName = newName.Replace(" ", "");

            if (newName.Length > MaxNameLength)
            {
                throw new InvalidDataException("Name was too long.");
            }
            
            var user = await usersRepository.GetByIdAsync(whoCalled);

            if (user == null)
            {
                throw new KeyNotFoundException("");
            }

            await usersRepository.ChangeLastName(newName, user);
            await usersRepository.UpdateAsync(user);
            await unitOfWork.Commit();
        }

        public async Task ChangeUsername(string newName, string whoCalled)
        {
            if (newName == null)
            {
                throw new InvalidDataException("New name cannot be null");
            }
            
            newName = newName.Replace(" ", "");

            if (newName.Length > MaxNameLength || newName.Length < MinNameLength)
            {
                throw new InvalidDataException("Name was too long or too short.");
            }

            if(newName.ToLower() == "admin")
            {
                throw new InvalidDataException("Forbidden username.");
            }

            var foundUser = await usersRepository.GetByUsername(newName);

            if (foundUser != null)
            {
                throw new InvalidDataException("New username was not unique.");
            }

            var user = await usersRepository.GetByIdAsync(whoCalled);

            if (user == null)
            {
                throw new KeyNotFoundException("");
            }
            
            await usersRepository.ChangeUsername(newName, user);
            await usersRepository.UpdateAsync(user);
            await unitOfWork.Commit();
        }

        public async Task<List<AppUserDto>> GetContacts(string callerId)
        {
            var caller = await usersRepository.GetByIdAsync(callerId);

            if (caller == null)
            {
                throw new InvalidDataException("caller id was wrong.");
            }

            return (await contactsRepository.ListAsync(new GetContactsOfSpec(callerId)))?
                .Select(x => x.Contact.ToAppUserDto())
                ?.ToList();
        }

        public async Task AddToContacts(string userId, string callerId)
        {
            if (userId == callerId)
            {
                throw new InvalidDataException("Can't add yourself to contacts.");
            }

            try
            {
                await contactsRepository.AddAsync(ContactsDataModel.Create(callerId, userId));
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
                var entry = await contactsRepository.GetByIdAsync(caller, userId);
                await contactsRepository.DeleteAsync(entry);
                await unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Wrong caller id or user id", ex);
            }
        }

        public async Task<UpdateProfilePictureResponse> UpdateThumbnail(IFormFile image, string userId)
        {
            if (image.Length / (1024 * 1024) > MaxThumbnailLengthMB)
            {
                throw new InvalidDataException($"Thumbnail was larger than {MaxThumbnailLengthMB}");
            }
            var user = await usersRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User was not found");
            }

            try
            {
                var (thumbnail, fullsized) = await imagesService.SaveProfileOrChatPicture(image, image.FileName, userId, userId);

                await usersRepository.UpdateAvatar(thumbnail, fullsized, user);
                await usersRepository.UpdateAsync(user);
                await unitOfWork.Commit();

                return new UpdateProfilePictureResponse
                {
                    ThumbnailUrl = thumbnail,
                    FullImageUrl = fullsized
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update profile picture. Try different one.", ex);
            }
        }

        public async Task<UsersByNickNameResultApiModel> FindUsersByNickName(string name)
        {
            if (name == null)
            {
                throw new InvalidDataException("Nickname was null");
            }

            var result = (await usersRepository.FindByUsername(name)).ToList();

            if (!result.Any())
            {
                return new UsersByNickNameResultApiModel
                {
                    UsersFound = null
                };
            }

            return new UsersByNickNameResultApiModel
            {
                UsersFound = result.Select(foundUser =>
                    foundUser.ToAppUserDto()
                ).ToList()
            };
        }

        public async Task ChangeUserIsPublicState(string userId)
        {
            var user = await usersRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException();
            }
            
            await usersRepository.ChangeUserPublicState(user);
            await usersRepository.UpdateAsync(user);
            await unitOfWork.Commit();
        }

        /// <summary>
        /// Updates IsOnline user field and connectionId, if no open connection is found.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="signalRConnectionId"></param>
        /// <returns></returns>
        public async Task MakeUserOnline(string userId, string signalRConnectionId)
        {
            var user = await usersRepository.GetByIdAsync(userId);

            if (user.ConnectionId != null)
            {
                await usersRepository.MakeUserOnline(user);   
            }
            else
            {
                await usersRepository.MakeUserOnline(user, true, signalRConnectionId);   
            }

            await usersRepository.UpdateAsync(user);
            await unitOfWork.Commit();
        }

        public async Task MakeUserOffline(string userId)
        {
            var user = await usersRepository.GetByIdAsync(userId);
            await usersRepository.MakeUserOffline(user);
            await usersRepository.UpdateAsync(user);
            await unitOfWork.Commit();
        }
    }
}