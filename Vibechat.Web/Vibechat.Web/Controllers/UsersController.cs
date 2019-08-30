using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.BusinessLogic.Services.Bans;
using Vibechat.BusinessLogic.Services.Users;
using Vibechat.Shared.ApiModels;
using Vibechat.Shared.ApiModels.Users_Info;
using Vibechat.Shared.DTO.Users;

namespace Vibechat.Web.Controllers
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


        [Authorize(Policy = "PublicApi")]
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                var user = await mUsersService.GetUserById(id, thisUserId);

                return Ok(new ResponseApiModel<AppUserDto>
                {
                    IsSuccessfull = true,
                    Response = user
                });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new ResponseApiModel<AppUserDto>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseApiModel<AppUserDto>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseApiModel<AppUserDto>
                {
                    IsSuccessfull = false
                });
            }
        }

        [Authorize(Policy = "PublicApi")]
        [HttpPatch]
        [Route("[action]")]
        public async Task<IActionResult> ChangeName([FromBody] ChangeNameRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mUsersService.ChangeName(request.newName, thisUserId);

                return Ok(new ResponseApiModel<bool>
                {
                    IsSuccessfull = true
                });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseApiModel<bool>
                {
                    IsSuccessfull = false
                });
            }
        }

        [Authorize(Policy = "PublicApi")]
        [HttpPatch]
        [Route("[action]")]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeNameRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mUsersService.ChangeUsername(request.newName, thisUserId);
  
                return Ok(new ResponseApiModel<bool>
                {
                    IsSuccessfull = true
                });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseApiModel<bool>
                {
                    IsSuccessfull = false
                });
            }
        }

        [Authorize(Policy = "PublicApi")]
        [HttpPut]
        [Route("[action]")]
        public async Task<IActionResult> ChangeInfo([FromBody] UpdateUserInfoRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mUsersService.UpdateUserInfo(request.UserName, request.FirstName, request.LastName, thisUserId);

                return Ok(new ResponseApiModel<bool>
                {
                    IsSuccessfull = true
                });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseApiModel<bool>
                {
                    IsSuccessfull = false
                });
            }
        }

        [Authorize(Policy = "PublicApi")]
        [HttpPatch]
        [Route("[action]")]
        public async Task<IActionResult> ChangeLastName([FromBody] ChangeNameRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mUsersService.ChangeLastName(request.newName, thisUserId);

                return Ok(new ResponseApiModel<bool>
                {
                    IsSuccessfull = true
                });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseApiModel<bool>
                {
                    IsSuccessfull = false
                });
            }
        }

        [Authorize(Policy = "PublicApi")]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> FindByNickName(
            [FromBody] SearchUsersRequest credentials)
        {
            try
            {
                var result = await mUsersService.FindUsersByNickName(credentials.UsernameToFind);

                return Ok(new ResponseApiModel<UsersByNickNameResultApiModel>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseApiModel<bool>
                {
                    IsSuccessfull = false
                });
            }
        }


        [Authorize(Policy = "PublicApi")]
        [HttpPatch]
        [Route("[action]")]
        public async Task<IActionResult> ChangePublicState()
        {
            try
            {
                await mUsersService.ChangeUserIsPublicState(JwtHelper.GetNamedClaimValue(User.Claims));

                return Ok(new ResponseApiModel<bool>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = true
                });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseApiModel<bool>
                {
                    IsSuccessfull = false
                });
            }
        }

        [Authorize(Policy = "PublicApi")]
        [HttpPatch]
        [Route("[action]")]
        public async Task<IActionResult> UpdateProfilePicture(
            [FromForm] UpdateProfilePictureRequest request)
        {
            try
            {
                var result =
                    await mUsersService.UpdateThumbnail(request.picture, JwtHelper.GetNamedClaimValue(User.Claims));

                return Ok(new ResponseApiModel<UpdateProfilePictureResponse>
                {
                    IsSuccessfull = true,
                    Response = result
                });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseApiModel<bool>
                {
                    IsSuccessfull = false
                });
            }
        }
        

        [Authorize(Policy = "PrivateApi")]
        [HttpPatch]
        [Route("{userId}/[action]")]
        public async Task<string> Lockout(string userId)
        {
            try
            {
                await BansService.LockoutUser(userId);
                return "";
            }
            catch (Exception e)
            {
                return $"Failed because of : {e.Message}";
            }
        }
        
        [Authorize(Policy = "PrivateApi")]
        [HttpPatch]
        [Route("{userId}/[action]")]
        public async Task<string> DisableLockout(string userId)
        {
            try
            {
                await BansService.DisableLockout(userId);
                return "";
            }
            catch (Exception e)
            {
                return $"Failed because of : {e.Message}";
            }
        }
    }
}