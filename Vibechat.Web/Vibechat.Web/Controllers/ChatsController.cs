using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("[action]")]
        public async Task<ResponseApiModel<Chat>> Create([FromBody] CreateChatRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);
                request.CreatorId = thisUserId;

                var result = await mChatsService.CreateConversation(request);

                return new ResponseApiModel<Chat>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<Chat>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPatch]
        [Route("{chatId:int}/[action]")]
        public async Task<ResponseApiModel<bool>> UpdateAuthKey([FromBody] UpdateAuthKeyRequest request, int chatId)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mChatsService.UpdateAuthKey(chatId, request.AuthKeyId, request.deviceId,
                    thisUserId);

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
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("{chatId:int}/[action]")]
        public async Task<ResponseApiModel<UsersByNickNameResultApiModel>> FindUsers(
            [FromBody] FindUsersInChatRequest credentials, int chatId)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                var result =
                    await mChatsService.FindUsersInChat(chatId, credentials.UsernameToFind,
                        thisUserId);

                return new ResponseApiModel<UsersByNickNameResultApiModel>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = new UsersByNickNameResultApiModel {UsersFound = result}
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<UsersByNickNameResultApiModel>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("[action]/{deviceId}")]
        public async Task<ResponseApiModel<List<Chat>>> GetAll(string deviceId)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                var result = await mChatsService.GetChats(deviceId, thisUserId);

                return new ResponseApiModel<List<Chat>>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<List<Chat>>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{chatId:int}")] // Chats/12
        public async Task<ResponseApiModel<Chat>> GetById(int chatId)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                var result = await mChatsService.GetById(chatId, thisUserId);

                return new ResponseApiModel<Chat>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<Chat>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }
        

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{chatId:int}/Participants")]
        public async Task<ResponseApiModel<List<UserInfo>>> GetParticipants(int chatId)
        {
            try
            {
                var result = await mChatsService.GetParticipants(chatId);

                return new ResponseApiModel<List<UserInfo>>
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<List<UserInfo>>
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPatch]
        [Route("{chatId:int}/[action]")]
        public async Task<ResponseApiModel<UpdateThumbnailResponse>> UpdateThumbnail(
            [FromForm] UpdateThumbnailRequest updateThumbnail, int chatId)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                var result = await mChatsService.UpdateThumbnail(chatId,
                    updateThumbnail.thumbnail, thisUserId);
                return new ResponseApiModel<UpdateThumbnailResponse>
                {
                    IsSuccessfull = true,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<UpdateThumbnailResponse>
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPatch]
        [Route("{chatId:int}/[action]")]
        public async Task<ResponseApiModel<bool>> ChangeName([FromBody] ChangeChatNameRequest request,
            int chatId)
        {
            try
            {
                await mChatsService.ChangeName(chatId, request.Name);

                return new ResponseApiModel<bool>
                {
                    IsSuccessfull = true
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
        [HttpPost]
        [Route("[action]")]
        public async Task<ResponseApiModel<List<Chat>>> Search([FromBody] SearchRequest request)
        {
            try 
            {
                var result = await mChatsService.SearchForGroups(request.SearchString,
                    JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<List<Chat>>
                {
                    IsSuccessfull = true,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<List<Chat>>
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPatch]
        [Route("{chatId:int}/[action]")]
        public async Task<ResponseApiModel<bool>> ChangePublicState(
            int chatId)
        {
            try
            {
                await mChatsService.ChangePublicState(chatId,
                    JwtHelper.GetNamedClaimValue(User.Claims));

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
        [HttpPost]
        [Route("{chatId:int}/[action]")]
        public async Task<ResponseApiModel<bool>> BanFrom([FromBody] BanRequest request, int chatId)
        {
            try
            {
                await BansService.BanUserFromConversation(
                    chatId,
                    request.userId,
                    JwtHelper.GetNamedClaimValue(User.Claims));

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
        [HttpPost]
        [Route("{chatId:int}/[action]")]
        public async Task<ResponseApiModel<bool>> UnbanFrom([FromBody] BanRequest request, int chatId)
        {
            try
            {
                await BansService.UnbanUserFromConversation(
                    chatId,
                    request.userId,
                    JwtHelper.GetNamedClaimValue(User.Claims));

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