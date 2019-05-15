using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.Services;
using Vibechat.Web.Services.Users;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;

namespace VibeChat.Web.Controllers
{
    public class UsersController : Controller
    {
        protected UsersInfoService mUsersService;

        public UsersController(UsersInfoService mDbService)
        {
            this.mUsersService = mDbService;
        }

        #region Users info
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
        #endregion
    }
}
