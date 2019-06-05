using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.Services;
using Vibechat.Web.Services.Bans;
using Vibechat.Web.Services.Users;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;

namespace VibeChat.Web.Controllers
{
    public class UsersController : Controller
    {
        protected UsersInfoService mUsersService;

        public BansService BansService { get; }

        public UsersController(UsersInfoService mDbService, BansService bansService)
        {
            this.mUsersService = mDbService;
            BansService = bansService;
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/GetById")]
        public async Task<ResponseApiModel<UserInfo>> GetById([FromBody]UserByIdApiModel request)
        {
            try
            {
                UserInfo user = await mUsersService.GetUserById(request);

                user.IsMessagingRestricted = BansService.IsBannedFromMessagingWith(JwtHelper.GetNamedClaimValue(User.Claims), request.Id);

                user.IsBlocked = BansService.IsBannedFromMessagingWith(request.Id, JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<UserInfo>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = user
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<UserInfo>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }

        public class ChangeNameRequest
        {
            public string newName { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/ChangeName")]
        public async Task<ResponseApiModel<bool>> ChangeName([FromBody] ChangeNameRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mUsersService.ChangeName(request.newName, thisUserId);

                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>()
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

                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>()
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/FindByNickname")]
        public async Task<ResponseApiModel<UsersByNickNameResultApiModel>> FindByNickName([FromBody]UsersByNickNameApiModel credentials)
        {
            try
            {
                var result = await mUsersService.FindUsersByNickName(credentials);

                return new ResponseApiModel<UsersByNickNameResultApiModel>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<UsersByNickNameResultApiModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }

        public class ChangeUserIsPublicStateRequest
        {
            public string userId { get; set; }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/ChangePublicState")]
        public async Task<ResponseApiModel<bool>> ChangeUserIsPublicState([FromBody]ChangeUserIsPublicStateRequest request)
        {
            try
            {
               await mUsersService.ChangeUserIsPublicState(
                   request.userId, 
                   JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = false
                };
            }
        }

        public class BlockRequest
        {
            public string userId { get; set; }

            //there could not exist a conversation with user we want to block.
            public int? conversationId { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/Block")]
        public async Task<ResponseApiModel<bool>> Block([FromBody]BlockRequest request)
        {
            try
            {
                string thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await BansService.BanUser(request.userId, thisUserId);

                if(request.conversationId != null)
                {
                    await BansService.BanUserFromConversation(request.conversationId.Value, request.userId, thisUserId);
                }

                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = false
                };
            }
        }

        public class UpdateProfilePictureResponse
        {
            public string ThumbnailUrl { get; set; }

            public string FullImageUrl { get; set; }
        }

        public class UpdateProfilePictureRequest
        {
            [FromForm(Name = "picture")]
            public IFormFile picture { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/UpdateProfilePicture")]
        public async Task<ResponseApiModel<UpdateProfilePictureResponse>> UpdateProfilePicture([FromForm]UpdateProfilePictureRequest request)
        {
            try
            {
                var result = await mUsersService.UpdateThumbnail(request.picture, JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<UpdateProfilePictureResponse>()
                {
                    IsSuccessfull = true,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<UpdateProfilePictureResponse>()
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        public class UserInfoRequest
        {
            public string userId { get; set; }
        }


        public class IsBannedRequest
        {
            public string userid { get; set; }

            public string byWhom { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/isBanned")]
        public async Task<ResponseApiModel<bool>> IsBanned([FromBody]IsBannedRequest request)
        {
            try
            {
                var result = BansService.IsBannedFromMessagingWith(
                     request.userid,
                     request.byWhom);

                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = true,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>()
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        public class UnbanRequest
        {
            /// <summary>
            /// user to unban
            /// </summary>
            public string userId { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/Unban")]
        public async Task<ResponseApiModel<bool>> Unban([FromBody]UnbanRequest request)
        {
            try
            {
                await BansService.UnbanDialog(
                    request.userId,
                    JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = true,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>()
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }
    }
}
