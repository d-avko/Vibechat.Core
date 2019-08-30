using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.BusinessLogic.Services.Users;
using Vibechat.Shared.ApiModels;
using Vibechat.Shared.DTO.Users;

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
        
        [Authorize(Policy = "PublicApi")]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await mUsersService.GetContacts(JwtHelper.GetNamedClaimValue(User.Claims));

                return Ok(new ResponseApiModel<List<AppUserDto>>
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
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ResponseApiModel<bool>
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
        [Route("{userId}/[action]")]
        public async Task<IActionResult> Add(string userId)
        {
            try
            {
                await mUsersService.AddToContacts(userId, JwtHelper.GetNamedClaimValue(User.Claims));

                return Ok(new ResponseApiModel<bool>
                {
                    IsSuccessfull = true,
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
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ResponseApiModel<bool>
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
        [HttpDelete]
        [Route("{userId}/[action]")]
        public async Task<IActionResult> Remove(string userId)
        {
            try
            {
                await mUsersService.RemoveFromContacts(userId, JwtHelper.GetNamedClaimValue(User.Claims));

                return Ok(new ResponseApiModel<bool>
                {
                    IsSuccessfull = true,
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
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ResponseApiModel<bool>
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
    }
}