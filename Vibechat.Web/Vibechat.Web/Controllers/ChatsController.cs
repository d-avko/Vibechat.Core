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
        public async Task<ResponseApiModel<Chat>> Create([FromBody] CreateConversationCredentialsApiModel convInfo)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);
                convInfo.CreatorId = thisUserId;

                var result = await mChatsService.CreateConversation(convInfo);

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
        [HttpPost]
        [Route("[action]")]
        public async Task<ResponseApiModel<bool>> UpdateAuthKey([FromBody] UpdateAuthKeyRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mChatsService.UpdateAuthKey(request.chatId, request.AuthKeyId, request.deviceId,
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
        [Route("[action]")]
        public async Task<ResponseApiModel<UsersByNickNameResultApiModel>> FindUsersInChat(
            [FromBody] FindUsersInChatRequest credentials)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                var result =
                    await mChatsService.FindUsersInChat(credentials.ChatId, credentials.UsernameToFind,
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

                var result = await mChatsService.GetConversations(deviceId, thisUserId);

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
        [Route("Participants/{chatId:int}")]
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
        [HttpPost]
        [Route("[action]")]
        public async Task<ResponseApiModel<UpdateThumbnailResponse>> UpdateThumbnail(
            [FromForm] UpdateThumbnailRequest updateThumbnail)
        {
            try
            {
                var thisUserID = JwtHelper.GetNamedClaimValue(User.Claims);

                var result = await mChatsService.UpdateThumbnail(updateThumbnail.conversationId,
                    updateThumbnail.thumbnail, thisUserID);
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
        [HttpPost]
        [Route("[action]")]
        public async Task<ResponseApiModel<bool>> ChangeName([FromBody] ChangeConversationNameRequest request)
        {
            try
            {
                await mChatsService.ChangeName(request.ConversationId, request.Name);

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
        [HttpPost]
        [Route("[action]")]
        public async Task<ResponseApiModel<bool>> ChangePublicState(
            [FromBody] ChangeConversationPublicStateRequest request)
        {
            try
            {
                await mChatsService.ChangePublicState(request.conversationId,
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
        [Route("[action]")]
        public async Task<ResponseApiModel<bool>> BanFrom([FromBody] BanRequest request)
        {
            try
            {
                await BansService.BanUserFromConversation(
                    request.conversationId,
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
        [Route("[action]")]
        public async Task<ResponseApiModel<bool>> UnbanFrom([FromBody] BanRequest request)
        {
            try
            {
                await BansService.UnbanUserFromConversation(
                    request.conversationId,
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