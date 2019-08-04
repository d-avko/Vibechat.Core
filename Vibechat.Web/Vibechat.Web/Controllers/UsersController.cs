using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vibechat.Web.ApiModels;
using VibeChat.Web.ChatData;
using Vibechat.Web.Services.Bans;
using Vibechat.Web.Services.Users;

namespace VibeChat.Web.Controllers
{
    public class UsersController : Controller
    {
        protected UsersService mUsersService;

        public UsersController(UsersService mDbService, BansService bansService)
        {
            mUsersService = mDbService;
            BansService = bansService;
        }

        public BansService BansService { get; }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/GetById")]
        public async Task<ResponseApiModel<UserInfo>> GetById([FromBody] UserByIdApiModel request)
        {
            try
            {
                var user = await mUsersService.GetUserById(request);

                user.IsMessagingRestricted =
                    BansService.IsBannedFromMessagingWith(JwtHelper.GetNamedClaimValue(User.Claims), request.Id);

                user.IsBlocked =
                    BansService.IsBannedFromMessagingWith(request.Id, JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<UserInfo>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = user
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<UserInfo>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/ChangeName")]
        public async Task<ResponseApiModel<bool>> ChangeName([FromBody] ChangeNameRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mUsersService.ChangeName(request.newName, thisUserId);

                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/ChangeUsername")]
        public async Task<ResponseApiModel<bool>> ChangeUsername([FromBody] ChangeNameRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mUsersService.ChangeUsername(request.newName, thisUserId);

                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/ChangeInfo")]
        public async Task<ResponseApiModel<bool>> ChangeUsername([FromBody] UpdateUserInfoRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mUsersService.UpdateUserInfo(request.UserName, request.FirstName, request.LastName, thisUserId);

                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/ChangeLastName")]
        public async Task<ResponseApiModel<bool>> ChangeLastName([FromBody] ChangeNameRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mUsersService.ChangeLastName(request.newName, thisUserId);

                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/FindByNickname")]
        public async Task<ResponseApiModel<UsersByNickNameResultApiModel>> FindByNickName(
            [FromBody] UsersByNickNameApiModel credentials)
        {
            try
            {
                var result = await mUsersService.FindUsersByNickName(credentials);

                return new ResponseApiModel<UsersByNickNameResultApiModel>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<UsersByNickNameResultApiModel>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/ChangePublicState")]
        public async Task<ResponseApiModel<bool>> ChangeUserIsPublicState(
            [FromBody] ChangeUserIsPublicStateRequest request)
        {
            try
            {
                await mUsersService.ChangeUserIsPublicState(
                    request.userId,
                    JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/Block")]
        public async Task<ResponseApiModel<bool>> Block([FromBody] BlockRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await BansService.BanDialog(request.userId, thisUserId);

                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/UpdateProfilePicture")]
        public async Task<ResponseApiModel<UpdateProfilePictureResponse>> UpdateProfilePicture(
            [FromForm] UpdateProfilePictureRequest request)
        {
            try
            {
                var result =
                    await mUsersService.UpdateThumbnail(request.picture, JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<UpdateProfilePictureResponse>
                {
                    IsSuccessfull = true,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<UpdateProfilePictureResponse>
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/GetContacts")]
        public async Task<ResponseApiModel<List<UserInfo>>> GetContacts()
        {
            try
            {
                var result = await mUsersService.GetContacts(JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<List<UserInfo>>
                {
                    IsSuccessfull = true,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<List<UserInfo>>
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/AddToContacts")]
        public async Task<ResponseApiModel<bool>> AddToContacts([FromBody] UserInfoRequest request)
        {
            try
            {
                await mUsersService.AddToContacts(request.userId, JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = true,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/RemoveFromContacts")]
        public async Task<ResponseApiModel<bool>> RemoveFromContacts([FromBody] UserInfoRequest request)
        {
            try
            {
                await mUsersService.RemoveFromContacts(request.userId, JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = true,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/isBanned")]
        public async Task<ResponseApiModel<bool>> IsBanned([FromBody] IsBannedRequest request)
        {
            try
            {
                var result = BansService.IsBannedFromMessagingWith(
                    request.userid,
                    request.byWhom);

                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = true,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/Unban")]
        public async Task<ResponseApiModel<bool>> Unban([FromBody] UnbanRequest request)
        {
            try
            {
                await BansService.UnbanDialog(
                    request.userId,
                    JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = true,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }
        
    }
}