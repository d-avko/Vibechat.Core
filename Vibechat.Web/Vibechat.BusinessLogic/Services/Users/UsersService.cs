using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;
using Vibechat.BusinessLogic.Extensions;
using Vibechat.BusinessLogic.Services.Bans;
using Vibechat.BusinessLogic.Services.Connections;
using Vibechat.BusinessLogic.Services.FileSystem;
using Vibechat.DataLayer;
using Vibechat.DataLayer.DataModels;
using Vibechat.DataLayer.Repositories;
using Vibechat.DataLayer.Repositories.Specifications.Contacts;
using Vibechat.Shared.ApiModels;
using Vibechat.Shared.ApiModels.Users_Info;
using Vibechat.Shared.DTO.Users;

namespace Vibechat.BusinessLogic.Services.Users
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
        private readonly ConnectionsService _connectionsService;

        public UsersService(
            IUsersRepository usersRepository,
            FilesService imagesService,
            IContactsRepository contactsRepository,
            UnitOfWork unitOfWork,
            BansService bansService,
            ConnectionsService connectionsService)
        {
            this.usersRepository = usersRepository;
            this.imagesService = imagesService;
            this.contactsRepository = contactsRepository;
            this.unitOfWork = unitOfWork;
            this.bansService = bansService;
            _connectionsService = connectionsService;
        }

        public async Task<AppUserDto> GetUserById(string userId, string callerId)
        {
            if (userId == null)
            {
                throw new InvalidDataException("Provided userId was null");
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
            await usersRepository.MakeUserOnline(user);

            if(user.Connections.FirstOrDefault(c => c.ConnectionId == signalRConnectionId) == default)
            {
                var newConnection = await _connectionsService.AddConnection(signalRConnectionId, userId);

                if (newConnection == null)
                {
                    return;
                }
                
                user.Connections.Add(newConnection);
            }

            await usersRepository.UpdateAsync(user);
            await unitOfWork.Commit();
        }

        public async Task MakeUserOffline(string userId, string signalRConnectionId)
        {
            var user = await usersRepository.GetByIdAsync(userId);

            //if connection is not already deleted, delete it.
            if (user.Connections.FirstOrDefault(c => c.ConnectionId == signalRConnectionId) != default)
            {
                if (!await _connectionsService.RemoveConnection(signalRConnectionId, userId))
                {
                    return;
                }
                
                user.Connections.Remove(new UserConnectionDataModel() {ConnectionId = signalRConnectionId});
            }

            //last connection was deleted, user is offline.
            if(user.Connections.Count.Equals(0))
            {
                await usersRepository.MakeUserOffline(user);
            }

            await usersRepository.UpdateAsync(user);
            await unitOfWork.Commit();
        }
    }
}