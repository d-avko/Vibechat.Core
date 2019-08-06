using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeChat.Web;
using Vibechat.Web.ApiModels;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;
using Vibechat.Web.Data.ApiModels.Messages;
using Vibechat.Web.Services.Messages;

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
        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("[action]")]
        public async Task<ResponseApiModel<List<Message>>> GetAttachments([FromBody] GetAttachmentsRequest request)
        {
            try
            {
                var result = await messagesService.GetAttachments(
                    request.kind,
                    request.conversationId,
                    JwtHelper.GetNamedClaimValue(User.Claims),
                    request.offset,
                    request.count);

                return new ResponseApiModel<List<Message>>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<List<Message>>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }
        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("[action]")]
        public async Task<ResponseApiModel<List<Message>>> Get([FromBody] GetMessagesApiModel credentials)
        { 
            try
            {
                var result = await messagesService.GetMessages(
                    credentials.ConversationID,
                    credentials.MessagesOffset,
                    credentials.Count,
                    credentials.MaxMessageId,
                    JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<List<Message>>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<List<Message>>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }
        
        /// <summary>
        /// Delete many entities by one request, as DELETE doesn't allow body.
        /// </summary>
        /// <param name="messagesInfo"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("[action]")]
        public async Task<ResponseApiModel<string>> Delete(
            [FromBody] DeleteMessagesRequest messagesInfo)
        { 
            try
            {
                await messagesService.DeleteMessages(
                    messagesInfo,
                    JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<string>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = null
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<string>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }
        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("[action]")]
        public async Task<ResponseApiModel<bool>> SetLast([FromBody] SetLastMessageRequest request)
        { 
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await messagesService.SetLastMessage(thisUserId, request.chatId, request.messageId);

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
    }
}