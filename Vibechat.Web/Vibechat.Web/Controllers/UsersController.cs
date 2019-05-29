using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<ResponseApiModel<UserByIdApiResponseModel>> GetById([FromBody]UserByIdApiModel userId)
        {
            try
            {
                var result = await mUsersService.GetUserById(userId);

                return new ResponseApiModel<UserByIdApiResponseModel>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<UserByIdApiResponseModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
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
                   JwtHelper.GetNamedClaim(User.Claims));

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

            public int? conversationId { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/Block")]
        public async Task<ResponseApiModel<bool>> Block([FromBody]BlockRequest request)
        {
            try
            {
                await BansService.BanUser(request.userId, request.conversationId, JwtHelper.GetNamedClaim(User.Claims));

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
                await BansService.Unban(
                    request.userId,
                    JwtHelper.GetNamedClaim(User.Claims));

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
