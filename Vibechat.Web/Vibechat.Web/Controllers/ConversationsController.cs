﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vibechat.Web.ApiModels;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;
using Vibechat.Web.Data.ApiModels.Conversation;
using Vibechat.Web.Data.ApiModels.Messages;
using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Data.Messages;
using Vibechat.Web.Services;
using Vibechat.Web.Services.Bans;

namespace VibeChat.Web.Controllers
{
    public class ConversationsController : Controller
    {
        protected ChatService mConversationService;

        public BansService BansService { get; }

        public ConversationsController(
            ChatService mDbService,
            BansService bansService)
        {
            this.mConversationService = mDbService;
            BansService = bansService;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/Create")]
        public async Task<ResponseApiModel<Chat>> Create([FromBody] CreateConversationCredentialsApiModel convInfo)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);
                convInfo.CreatorId = thisUserId;

                var result = await mConversationService.CreateConversation(convInfo);

                return new ResponseApiModel<Chat>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<Chat>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }

        public class UpdateAuthKeyRequest
        {
            public int chatId { get; set; }

            public string AuthKeyId { get; set; }

            public string deviceId { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/UpdateAuthKey")]
        public async Task<ResponseApiModel<bool>> UpdateAuthKey([FromBody] UpdateAuthKeyRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mConversationService.UpdateAuthKey(request.chatId, request.AuthKeyId, request.deviceId, thisUserId);

                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = true,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = false
                };
            }
        }

        public class FindUsersInChatRequest
        {
            public string UsernameToFind { get; set; }

            public int ChatId { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/FindUsersInChat")]
        public async Task<ResponseApiModel<UsersByNickNameResultApiModel>> FindUsersInChat([FromBody]FindUsersInChatRequest credentials)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                var result = await mConversationService.FindUsersInChat(credentials.ChatId, credentials.UsernameToFind, thisUserId);

                foreach (var user in result)
                {
                    user.IsBlockedInConversation = await BansService.IsBannedFromConversation(credentials.ChatId, user.Id);
                    user.ChatRole = await mConversationService.GetChatRole(user.Id, credentials.ChatId);
                }

                return new ResponseApiModel<UsersByNickNameResultApiModel>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = new UsersByNickNameResultApiModel() { UsersFound = result }
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<UsersByNickNameResultApiModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetRoles")]
        public async Task<ResponseApiModel<List<ChatRoleDto>>> GetRoles()
        {
            try 
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                var result = await mConversationService.GetChatRoles(thisUserId);

                return new ResponseApiModel<List<ChatRoleDto>>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<List<ChatRoleDto>>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }

        public class GetChatsRequest
        {
            public string deviceId { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetAll")]
        public async Task<ResponseApiModel<List<Chat>>> GetAll([FromBody] GetChatsRequest request)
        {
            try
            {
                var thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                List<Chat> result = await mConversationService.GetConversations(request.deviceId,  thisUserId);

                return new ResponseApiModel<List<Chat>>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<List<Chat>>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }

        public class GetByIdRequest
        {
            public int conversationId { get; set; }

            public bool updateRoles { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetById")]
        public async Task<ResponseApiModel<Chat>> GetById([FromBody]GetByIdRequest request)
        {
            try
            {
                string thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                Chat result = await mConversationService.GetById(request.conversationId, thisUserId);

                return new ResponseApiModel<Chat>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<Chat>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }
        
        public class SetLastMessageRequest
        {
            public int chatId { get; set; }
            
            public int messageId { get; set; }
        }
        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/SetLastMessage")]
        public async Task<ResponseApiModel<bool>> GetById([FromBody]SetLastMessageRequest request)
        {
            try
            {
                string thisUserId = JwtHelper.GetNamedClaimValue(User.Claims);

                await mConversationService.SetLastMessage(thisUserId, request.chatId, request.messageId);

                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = false
                };
            }
        }

        public class GetAttachmentsRequest
        {
            public AttachmentKind kind { get; set; }

            public int conversationId { get; set; }

            public int offset { get; set; }

            public int count { get; set; }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetAttachments")]
        public async Task<ResponseApiModel<List<Message>>> GetAttachments([FromBody] GetAttachmentsRequest request)
        {
            try
            {
                var result = await mConversationService.GetAttachments(
                    request.kind, 
                    request.conversationId, 
                    JwtHelper.GetNamedClaimValue(User.Claims),
                    request.offset,
                    request.count);

                return new ResponseApiModel<List<Message>>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<List<Message>>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetParticipants")]
        public async Task<ResponseApiModel<List<UserInfo>>> GetParticipants([FromBody] GetParticipantsApiModel convInfo)
        {
            try
            {
                List<UserInfo> result = await mConversationService.GetParticipants(convInfo.ConvId);

                return new ResponseApiModel<List<UserInfo>>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<List<UserInfo>>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = ex.Message,
                    Response = null
                };
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/GetMessages")]
        public async Task<ResponseApiModel<List<Message>>> GetMessages([FromBody] GetMessagesApiModel credentials)
        {
            try
            {
                List<Message> result = await mConversationService.GetMessages(
                    credentials.ConversationID,
                    credentials.MessagesOffset,
                    credentials.Count,
                    credentials.MaxMessageId,
                     JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<List<Message>>()
                {
                    IsSuccessfull = true,
                    ErrorMessage = null,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<List<Message>>()
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
                await mConversationService.DeleteConversationMessages(
                    messagesInfo,
                     JwtHelper.GetNamedClaimValue(User.Claims));

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
        [Route("api/Conversations/UpdateThumbnail")]
        public async Task<ResponseApiModel<UpdateThumbnailResponse>> UpdateThumbnail([FromForm]UpdateThumbnailRequest updateThumbnail)
        {
            try
            {
                var thisUserID = JwtHelper.GetNamedClaimValue(User.Claims);

                var result = await mConversationService.UpdateThumbnail(updateThumbnail.conversationId, updateThumbnail.thumbnail, thisUserID);
                return new ResponseApiModel<UpdateThumbnailResponse>()
                {
                    IsSuccessfull = true,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<UpdateThumbnailResponse>()
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/ChangeName")]
        public async Task<ResponseApiModel<bool>> ChangeName([FromBody] ChangeConversationNameRequest request)
        {
            try
            {
                await mConversationService.ChangeName(request.ConversationId, request.Name);

                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>()
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        public class BanRequest
        {
            public string userId { get; set; }

            public int conversationId { get; set; }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/SearchGroups")]
        public async Task<ResponseApiModel<List<Chat>>> SearchGroups([FromBody] SearchRequest request)
        {
            try
            {
                var result = await mConversationService.SearchForGroups(request.SearchString,
                     JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<List<Chat>>()
                {
                    IsSuccessfull = true,
                    Response = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<List<Chat>>()
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        public class ChangeConversationPublicStateRequest
        {
            public int conversationId{ get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/ChangePublicState")]
        public async Task<ResponseApiModel<bool>> ChangeConversationPublicState([FromBody]ChangeConversationPublicStateRequest request)
        { 
            try
            {
               await mConversationService.ChangePublicState(request.conversationId,
                     JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = true,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>()
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/BanFrom")]
        public async Task<ResponseApiModel<bool>> BanFrom([FromBody] BanRequest request)
        {
            try
            {
                await BansService.BanUserFromConversation(
                    request.conversationId,
                    request.userId,
                    JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = true,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>()
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Conversations/UnbanFrom")]
        public async Task<ResponseApiModel<bool>> UnbanFrom([FromBody] BanRequest request)
        {
            try
            {
                await BansService.UnbanUserFromConversation(
                    request.conversationId,
                    request.userId,
                    JwtHelper.GetNamedClaimValue(User.Claims));

                return new ResponseApiModel<bool>()
                {
                    IsSuccessfull = true,
                    Response = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseApiModel<bool>()
                {
                    ErrorMessage = ex.Message,
                    IsSuccessfull = false
                };
            }
        }
    }
}
