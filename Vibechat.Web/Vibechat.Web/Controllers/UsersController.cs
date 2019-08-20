using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vibechat.Web.ApiModels;
using VibeChat.Web.ChatData;
using Vibechat.Web.Services.Bans;
using Vibechat.Web.Services.Users;

namespace VibeChat.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        protected UsersService mUsersService;

        public UsersController(UsersService mDbService, BansService bansService)
        {
            mUsersService = mDbService;
            BansService = bansService;
        }

        public BansService BansService { get; }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{id}")]
        public async Task<ResponseApiModel<UserInfo>> GetById(string id)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);
                
                var user = await mUsersService.GetUserById(id,thisUserId);

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
        [HttpPatch]
        [Route("[action]")]
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
        [HttpPatch]
        [Route("[action]")]
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
        [HttpPut]
        [Route("[action]")]
        public async Task<ResponseApiModel<bool>> ChangeInfo([FromBody] UpdateUserInfoRequest request)
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
        [HttpPatch]
        [Route("[action]")]
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
        [HttpPost]
        [Route("[action]")]
        public async Task<ResponseApiModel<UsersByNickNameResultApiModel>> FindByNickName(
            [FromBody] SearchUsersRequest credentials)
        {
            try
            {
                var result = await mUsersService.FindUsersByNickName(credentials.UsernameToFind);

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
        [HttpPatch]
        [Route("[action]")]
        public async Task<ResponseApiModel<bool>> ChangePublicState(
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
        [HttpPost]
        [Route("[action]")]
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
        [HttpPatch]
        [Route("[action]")]
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
        [HttpPost]
        [Route("[action]")]
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