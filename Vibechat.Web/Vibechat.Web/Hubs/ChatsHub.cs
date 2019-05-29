using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vibechat.Web.Services;
using Vibechat.Web.Services.Bans;
using Vibechat.Web.Services.Users;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;
using VibeChat.Web.UserProviders;

namespace VibeChat.Web
{
    /// <summary>
    /// SignalR main hub
    /// </summary>
    public class ChatsHub : Hub
    {
        private ICustomHubUserIdProvider userProvider { get; set; }

        private UsersInfoService userService { get; set; }

        private ConversationsInfoService conversationsService { get; set; }
        public BansService BansService { get; }
        private ILogger<ChatsHub> logger { get; set; }


        public ChatsHub(
            ICustomHubUserIdProvider userProvider, 
            UsersInfoService userService,
            ConversationsInfoService conversationsService,
            BansService bansService,
            ILogger<ChatsHub> logger)
        { 
            this.userProvider = userProvider;
            this.userService = userService;
            this.conversationsService = conversationsService;
            BansService = bansService;
            this.logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            await userService.MakeUserOnline(userProvider.GetUserId(Context), Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await userService.MakeUserOffline(userProvider.GetUserId(Context));
            await base.OnDisconnectedAsync(ex);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task RemoveFromGroup(string userToRemoveId, int conversationId, bool IsSelf)
        {
            var whoSentId = userProvider.GetUserId(Context);

            try
            {
                await conversationsService.RemoveUserFromConversation(userToRemoveId, whoSentId, conversationId, IsSelf);
                await RemovedFromGroup(userToRemoveId, conversationId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
            }
            catch(Exception ex)
            {
                await SendError(whoSentId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Used to add user to specified group
        /// </summary>
        /// <param name="userId">id of a user</param>
        /// <param name="conversationId">conversation to add to</param>
        /// <param name="connectionId"> connection to add</param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task AddToGroup(string userId, ConversationTemplate conversation)
        {
            UserInApplication whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            try
            {
                var addedUser = await conversationsService.AddUserToConversation(new AddToConversationApiModel()
                {
                    ConvId = conversation.ConversationID,
                    UserId = userId
                });

                if (addedUser.IsOnline)
                {
                    await Groups.AddToGroupAsync(addedUser.ConnectionId, conversation.ConversationID.ToString());
                }

                conversation.Participants.Add(addedUser);

                await AddedToGroup(addedUser, conversation, true);
            }
            catch(Exception ex)
            {
                await SendError(whoSent.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task RemoveConversation(ConversationTemplate conversation)
        {
            UserInApplication whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            try
            {
                await conversationsService.RemoveConversation(conversation, whoSent.Id);

                if (conversation.IsGroup)
                {
                    await RemovedFromGroup(conversation.DialogueUser.Id, conversation.ConversationID);
                    await RemovedFromGroup(whoSent.Id, conversation.ConversationID);
                }
                else
                {
                    foreach(UserInfo user in conversation.Participants)
                    {
                        UserInApplication userToSend = await userService.GetUserById(user.Id).ConfigureAwait(false);

                        if (userToSend.IsOnline)
                        {
                            await RemovedFromDialog(user.Id, userToSend.ConnectionId, conversation.ConversationID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await SendError(whoSent.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task CreateDialog(UserInfo user)
        {
            UserInApplication whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            try
            {
                ConversationTemplate created = await conversationsService.CreateConversation(new CreateConversationCredentialsApiModel()
                {
                    IsGroup = false,
                    DialogUserId = user.Id,
                    CreatorId = whoSent.Id
                });

                if (whoSent.IsOnline)
                {
                    //send to self 
                    await AddedToDialog(new UserInfo() { Id = whoSent.Id }, whoSent.ConnectionId, created);
                }

                UserInApplication userToSend = await userService.GetUserById(user.Id);

                if (userToSend.IsOnline)
                {
                    await AddedToDialog(new UserInfo() { Id = userToSend.Id }, userToSend.ConnectionId, created);
                }
            }
            catch (Exception ex)
            {
                await SendError(whoSent.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Used to connect to a group to start chatting
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task ConnectToGroups(List<int> groupIds)
        {
            foreach(var groupId in groupIds)
            {
                //CHECK IF THIS IS EVEN ALLOWED
                await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task SendMessageToGroup(Message message, int groupId)
        {
            UserInApplication whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            try
            {
                //CAN SEND MESSAGE ONLY IF JOINED

                var created = new MessageDataModel();

                if (message.IsAttachment)
                {
                    created = await conversationsService.AddAttachmentMessage(message, groupId, whoSent.Id);
                }
                else
                {
                    created = await conversationsService.AddMessage(message, groupId, whoSent.Id);
                }

                message.TimeReceived = created.TimeReceived;
                message.Id = created.MessageID;

                await SendMessageToGroup(groupId, whoSent.Id, message, true, true);
            }
            catch(Exception ex)
            {
                await SendError(whoSent.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Method to send a message.
        /// Using overrided user string (defined as DefaultUserProvider)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="SenderId"></param>
        /// <param name="UserToSendId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task SendMessageToUser(Message message, string UserToSendId, int conversationId)
        {
            UserInApplication whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            try
            {

                //CAN SEND TO ANYONE

                var created = new MessageDataModel();

                if (message.IsAttachment)
                {
                    created = await conversationsService.AddAttachmentMessage(message, conversationId, whoSent.Id);
                }
                else
                {
                    created = await conversationsService.AddMessage(message, conversationId, whoSent.Id);
                }

                message.TimeReceived = created.TimeReceived;
                message.Id = created.MessageID;

                var userToSend = await userService.GetUserById(UserToSendId);

                if (userToSend.IsOnline)
                {
                    await SendMessageToUser(message, whoSent.Id, userToSend.ConnectionId, conversationId, true, true);
                }

                if (whoSent.IsOnline)
                {
                    await SendMessageToUser(message, whoSent.Id, whoSent.ConnectionId, conversationId, true, true);
                }
            }
            catch(Exception ex)
            {
                await SendError(whoSent.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Used to notify users of leaving of specified member.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private async Task RemovedFromGroup(string userId, int conversationId)
        {
            await Clients.Group(conversationId.ToString()).SendAsync("RemovedFromGroup",userId, conversationId);
        }

        private async Task AddedToGroup(UserInfo user, ConversationTemplate conversation, bool x)
        {
            await Clients.Group(conversation.ConversationID.ToString()).SendAsync("AddedToGroup", conversation, user);
        }

        private async Task AddedToDialog(UserInfo user, string connectionId, ConversationTemplate conversation)
        {
            await Clients.Client(connectionId).SendAsync("AddedToGroup", conversation, user);
        }
        private async Task RemovedFromDialog(string userId, string connectionId, int conversationId)
        {
            await Clients.Client(connectionId).SendAsync("RemovedFromGroup", userId, conversationId);
        }

        private async Task SendMessageToGroup(int groupId, string SenderId, Message message, bool y, bool x)
        {
            await Clients.Group(groupId.ToString()).SendAsync("ReceiveMessage", SenderId, message, groupId);
        }

        private async Task SendMessageToUser(Message message, string SenderId, string UserToSendConnectionId, int conversationId, bool x, bool y)
        {
            await Clients.Client(UserToSendConnectionId).SendAsync("ReceiveMessage", SenderId, message, conversationId);
        }

        private async Task SendError(string userid, string error)
        {
            await Clients.Client(userid).SendAsync("Error", error);
        }
    }
}
