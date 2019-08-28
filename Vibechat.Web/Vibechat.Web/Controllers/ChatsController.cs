﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vibechat.Web.ApiModels;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;
using Vibechat.Web.Data.ApiModels.Conversation;
using Vibechat.Web.Services;
using Vibechat.Web.Services.Bans;

namespace VibeChat.Web.Controllers
{
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly ChatService mChatsService;

        public ChatsController(
            ChatService mDbService,
            BansService bansService)
        {
            mChatsService = mDbService;
            BansService = bansService;
        }

        public BansService BansService { get; }

        [Authorize(Policy = "PublicApi")]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Create([FromBody] CreateChatRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);
                request.CreatorId = thisUserId;

                var result = await mChatsService.CreateConversation(request);

                return Ok(new ResponseApiModel<Chat>
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
        [Route("{chatId:int}/[action]")]
        public async Task<IActionResult> UpdateAuthKey([FromBody] UpdateAuthKeyRequest request, int chatId)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mChatsService.UpdateAuthKey(chatId, request.AuthKeyId, request.deviceId,
                    thisUserId);

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
        [Route("{chatId:int}/[action]")]
        public async Task<IActionResult> FindUsers(
            [FromBody] FindUsersInChatRequest credentials, int chatId)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                var result =
                    await mChatsService.FindUsersInChat(chatId, credentials.UsernameToFind,
                        thisUserId);

                return Ok(new ResponseApiModel<UsersByNickNameResultApiModel>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = new UsersByNickNameResultApiModel {UsersFound = result}
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
        [HttpGet]
        [Route("[action]/{deviceId}")]
        public async Task<IActionResult> GetAll(string deviceId)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                var result = await mChatsService.GetChats(deviceId, thisUserId);

                return Ok(new ResponseApiModel<List<Chat>>
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
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseApiModel<bool>
                {
                    IsSuccessfull = false
                });
            }
        }

        [Authorize(Policy = "PublicApi")]
        [HttpGet]
        [Route("{chatId:int}")] // Chats/12
        public async Task<IActionResult> GetById(int chatId)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                var result = await mChatsService.GetById(chatId, thisUserId);

                return Ok(new ResponseApiModel<Chat>
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
        [HttpPatch]
        [Route("{chatId:int}/[action]")]
        public async Task<IActionResult> UpdateThumbnail(
            [FromForm] UpdateThumbnailRequest updateThumbnail, int chatId)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                var result = await mChatsService.UpdateThumbnail(chatId,
                    updateThumbnail.thumbnail, thisUserId);
                
                return Ok(new ResponseApiModel<UpdateThumbnailResponse>
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
        [HttpPatch]
        [Route("{chatId:int}/[action]")]
        public async Task<IActionResult> ChangeName([FromBody] ChangeChatNameRequest request,
            int chatId)
        {
            try
            {
                await mChatsService.ChangeName(chatId, request.Name);

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
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
        {
            try 
            {
                var result = await mChatsService.SearchForGroups(request.SearchString,
                    JwtHelper.GetNamedClaimValue(User.Claims));

                return Ok(new ResponseApiModel<List<Chat>>
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
        [HttpPatch]
        [Route("{chatId:int}/[action]")]
        public async Task<IActionResult> ChangePublicState(
            int chatId)
        {
            try
            {
                await mChatsService.ChangePublicState(chatId,
                    JwtHelper.GetNamedClaimValue(User.Claims));

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