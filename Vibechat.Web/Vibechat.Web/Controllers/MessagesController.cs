using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vibechat.BusinessLogic.AuthHelpers;
using Vibechat.BusinessLogic.Services.Messages;
using Vibechat.Shared.ApiModels;
using Vibechat.Shared.ApiModels.Messages;
using Vibechat.Shared.DTO.Messages;

namespace Vibechat.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessagesService messagesService;

        public MessagesController(
            MessagesService messagesService)
        {
            this.messagesService = messagesService;
        }
        
        [Authorize(Policy = "PublicApi")]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GetAttachments([FromBody] GetAttachmentsRequest request)
        {
            try
            {
                var result = await messagesService.GetAttachments(
                    request.kind,
                    request.conversationId,
                    ClaimsExtractor.GetUserIdClaim(User.Claims),
                    request.offset,
                    request.count);

                return Ok(new ResponseApiModel<List<Message>>
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
            catch (Exception ex)
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
        public async Task<IActionResult> Get([FromBody] GetMessagesRequest credentials)
        { 
            try
            {
                var result = await messagesService.GetMessages(
                    credentials.ConversationID,
                    credentials.MessagesOffset,
                    credentials.Count,
                    credentials.MaxMessageId,
                    credentials.History,
                    credentials.SetLastMessage,
                    ClaimsExtractor.GetUserIdClaim(User.Claims));

                return Ok(new ResponseApiModel<List<Message>>
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
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseApiModel<bool>
                {
                    IsSuccessfull = false
                });
            }
        }
        
        /// <summary>
        /// Delete many entities by one request, as DELETE doesn't allow body (at least js libraries)
        /// </summary>
        /// <param name="messagesInfo"></param>
        /// <returns></returns>
        [Authorize(Policy = "PublicApi")]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Delete(
            [FromBody] DeleteMessagesRequest messagesInfo)
        { 
            try
            {
                await messagesService.DeleteMessages(
                    messagesInfo.MessagesId,
                    messagesInfo.ConversationId,
                    ClaimsExtractor.GetUserIdClaim(User.Claims));

                return Ok(new ResponseApiModel<string>
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
        [HttpPut]
        [Route("[action]")]
        public async Task<IActionResult> SetLast([FromBody] SetLastMessageRequest request)
        { 
            try
            {
                var thisUserId = ClaimsExtractor.GetUserIdClaim(User.Claims);

                await messagesService.SetLastMessage(thisUserId, request.chatId, request.messageId);

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
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Search([FromBody] SearchMessagesRequest request)
        {
            try
            {
                var thisUserId = ClaimsExtractor.GetUserIdClaim(User.Claims);
            
                var messages = await messagesService.SearchForMessages(
                    request.deviceId, 
                    request.searchString, 
                    request.offset,
                    request.count,
                    thisUserId);
            
                return Ok(new ResponseApiModel<List<Message>>
                {
                    IsSuccessfull = true,
                    Response = messages
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