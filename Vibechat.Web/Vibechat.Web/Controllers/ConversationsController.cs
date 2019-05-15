using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using Vibechat.Web.Data.ApiModels.Messages;
using Vibechat.Web.Services;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;

namespace VibeChat.Web.Controllers
{
    public class ConversationsController : Controller
    {
        protected ConversationsInfoService mDbService;

        public ConversationsController(ConversationsInfoService mDbService)
        {
            this.mDbService = mDbService;
        }

        #region Conversations
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/Create")]
        public async Task<ResponseApiModel<ConversationTemplate>> Create([FromBody] CreateConversationCredentialsApiModel convInfo)
        {
            try
            {
                var result = await mDbService.CreateConversation(convInfo);

                return new ResponseApiModel<ConversationTemplate>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<ConversationTemplate>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/AddUserTo")]
        public async Task<ResponseApiModel<string>> AddUserTo([FromBody]AddToConversationApiModel UserProvided)
        {
            try
            {
                await mDbService.AddUserToConversation(UserProvided);

                return new ResponseApiModel<string>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = null
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<string>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetInfo")]
        public async Task<ResponseApiModel<ConversationInfoResultApiModel>> GetInfo([FromBody] CredentialsForConversationInfoApiModel UserProvided)
        {
            try
            {
                var result = await mDbService.GetConversationInformation(
                    UserProvided, 
                    User.Claims.FirstOrDefault(x => x.Type == JwtHelper.JwtUserIdClaimName)
                    .Value);

                return new ResponseApiModel<ConversationInfoResultApiModel>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<ConversationInfoResultApiModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetParticipants")]
        public async Task<ResponseApiModel<GetParticipantsResultApiModel>> GetParticipants([FromBody] GetParticipantsApiModel convInfo)
        {
            try
            {
                var result = await mDbService.GetConversationParticipants(convInfo);

                return new ResponseApiModel<GetParticipantsResultApiModel>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<GetParticipantsResultApiModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetMessages")]
        public async Task<ResponseApiModel<GetMessagesResultApiModel>> GetMessages([FromBody] GetMessagesApiModel convInfo)
        {
            try
            {
                var result = await mDbService.GetConversationMessages(
                    convInfo,
                    User.Claims.FirstOrDefault(x => x.Type == JwtHelper.JwtUserIdClaimName)
                    .Value);

                return new ResponseApiModel<GetMessagesResultApiModel>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<GetMessagesResultApiModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/DeleteMessages")]
        public async Task<ResponseApiModel<string>> DeleteConversationMessages([FromBody] DeleteMessagesRequest messagesInfo)
        {
            try
            {
                await mDbService.DeleteConversationMessages(
                    messagesInfo,
                    User.Claims.FirstOrDefault(x => x.Type == JwtHelper.JwtUserIdClaimName)
                    .Value);

                return new ResponseApiModel<string>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = null
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<string>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }
            
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetConversationInfoById")]
        public async Task<ResponseApiModel<GetConversationByIdResultApiModel>> GetConversationById([FromBody] GetConversationByIdApiModel convInfo)
        {
            try
            {
                var result = await mDbService.GetConversationById(
                    convInfo,
                    User.Claims.FirstOrDefault(x => x.Type == JwtHelper.JwtUserIdClaimName)
                    .Value);

                return new ResponseApiModel<GetConversationByIdResultApiModel>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<GetConversationByIdResultApiModel>()
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
