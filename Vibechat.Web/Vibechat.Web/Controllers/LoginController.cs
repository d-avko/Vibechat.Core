using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vibechat.BusinessLogic.Services.Login;
using Vibechat.Shared.ApiModels;
using Vibechat.Shared.ApiModels.Login;

namespace Vibechat.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly LoginService loginService;

        public LoginController(LoginService loginService)
        {
            this.loginService = loginService;
        }

        /// <summary>
        /// Signs in user using firebase-issued JWT token (obtained via SMS confirmation).
        /// </summary>
        /// <param name="loginCredentials"></param>
        /// <returns></returns>
        [Route("")]
        [HttpPost]
        public async Task<IActionResult> Login(
            [FromBody] LoginCredentialsApiModel loginCredentials)
        {
            try
            {
                var result = await loginService.LogInAsync(loginCredentials.UidToken,
                    loginCredentials.PhoneNumber);

                return Ok(new ResponseApiModel<LoginResultApiModel>
                {
                    IsSuccessfull = true,
                    Response = result
                });
            }
            catch (FirebaseAdmin.FirebaseException)
            {
                return BadRequest(new ResponseApiModel<bool>
                {
                    IsSuccessfull = false,
                    ErrorMessage = "Wrong firebase user token."
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