using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeChat.Web;
using Vibechat.Web.ApiModels;
using VibeChat.Web.ChatData;
using Vibechat.Web.Services.Users;

namespace Vibechat.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly UsersService mUsersService;

        public ContactsController(UsersService mUsersService)
        {
            this.mUsersService = mUsersService;
        }
        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("[action]")]
        public async Task<ResponseApiModel<List<UserInfo>>> Get()
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
        [HttpPost]
        [Route("{userId}/[action]")]
        public async Task<ResponseApiModel<bool>> Add(string userId)
        {
            try
            {
                await mUsersService.AddToContacts(userId, JwtHelper.GetNamedClaimValue(User.Claims));

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
        [HttpDelete]
        [Route("{userId}/[action]")]
        public async Task<ResponseApiModel<bool>> Remove(string userId)
        {
            try
            {
                await mUsersService.RemoveFromContacts(userId, JwtHelper.GetNamedClaimValue(User.Claims));

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