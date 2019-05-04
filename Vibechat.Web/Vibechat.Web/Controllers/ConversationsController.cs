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
using Vibechat.Web.Services;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;

namespace VibeChat.Web.Controllers
{
    public class ConversationsController : Controller
    {
        protected DatabaseService mDbService;

        public ConversationsController(DatabaseService mDbService)
        {
            this.mDbService = mDbService;
        }

        #region Conversations
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/CreateConversation")]
        public async Task<ResponseApiModel<CreateConversationResultApiModel>> CreateConversation([FromBody] CreateConversationCredentialsApiModel convInfo)
        {
            try
            {
                await mDbService.CreateConversation(convInfo);

                return new ResponseApiModel<CreateConversationResultApiModel>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = null
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<CreateConversationResultApiModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/AddToConversation")]
        public async Task<ResponseApiModel<string>> AddUserToConversation([FromBody]AddToConversationApiModel UserProvided)
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
        [Route("api/Conversations/GetConversationInfo")]
        public async Task<ResponseApiModel<ConversationInfoResultApiModel>> GetConversationInformation([FromBody] CredentialsForConversationInfoApiModel UserProvided)
        {
            try
            {
                var result = await mDbService.GetConversationInformation(
                    UserProvided, 
                    User.Claims.FirstOrDefault(x => x.Type == JwtHelper.JwtUserClaimName)
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
        [Route("api/Conversations/GetConversationParticipants")]
        public async Task<ResponseApiModel<GetParticipantsResultApiModel>> GetConversationParticipants([FromBody] GetParticipantsApiModel convInfo)
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
        [Route("api/Conversations/GetConversationMessages")]
        public async Task<ResponseApiModel<GetMessagesResultApiModel>> GetConversationMessages([FromBody] GetMessagesApiModel convInfo)
        {
            try
            {
                var result = await mDbService.GetConversationMessages(convInfo);

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
        [Route("api/Conversations/GetConversationInfoById")]
        public async Task<ResponseApiModel<GetConversationByIdResultApiModel>> GetConversationById([FromBody] GetConversationByIdApiModel convInfo)
        {
            try
            {
                var result = await mDbService.GetConversationById(convInfo);

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
