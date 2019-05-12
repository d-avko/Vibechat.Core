using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vibechat.Web.Services;
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

        private DatabaseService dbService { get; set; }

        private ILogger<ChatsHub> logger { get; set; }


        public ChatsHub(ICustomHubUserIdProvider userProvider, DatabaseService dbService, ILogger<ChatsHub> logger)
        { 
            this.userProvider = userProvider;
            this.dbService = dbService;
            this.logger = logger;
        }

        public async Task OnConnected()
        {
            await dbService.MakeUserOnline(userProvider.GetUserId(Context), Context.ConnectionId);
        }

        public async Task OnDisconnected()
        {
            await dbService.MakeUserOffline(userProvider.GetUserId(Context));       
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task RemoveFromGroup(string userToRemoveId, int conversationId)
        {
            try
            {
                await dbService.RemoveUserFromGroup();
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
                await RemovedFromGroup(userToRemoveId, conversationId, true);
            }
            catch(Exception ex)
            {
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
        public async Task AddToGroup(string userId, int conversationId, string connectionId)
        {
            try
            {
                await dbService.AddUserToGroup();
                await Groups.AddToGroupAsync(connectionId, conversationId.ToString());
                await AddedToGroup(userId, conversationId, true);
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Used to connect to a group to start chatting
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task ConnectToGroup(int groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task SendMessageToGroup(Message message, string SenderId, int groupId)
        {
            try
            {
                await SendMessageToGroup(groupId, SenderId, message, true, true);

                if (message.IsAttachment)
                {
                    await dbService.AddAttachmentMessage(message, groupId, SenderId);
                }
                else
                {
                    await dbService.AddMessage(message, groupId, SenderId);
                }
            }
            catch(Exception ex)
            {
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
        public async Task SendMessageToUser(Message message, string SenderId, string UserToSendId, int conversationId)
        {
            try
            {
                await SendMessageToUser(message, SenderId, UserToSendId, conversationId, true, true);

                if (message.IsAttachment)
                {
                    await dbService.AddAttachmentMessage(message, conversationId, SenderId);
                }
                else
                {
                    await dbService.AddMessage(message, conversationId, SenderId);
                }
            }
            catch(Exception ex)
            {
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
        private async Task RemovedFromGroup(string userId, int conversationId, bool x)
        {
            await Clients.Group(conversationId.ToString()).SendAsync("RemovedFromGroup", conversationId, userId);
        }

        private async Task AddedToGroup(string userId, int conversationId, bool x)
        {
            await Clients.Group(conversationId.ToString()).SendAsync("AddedToGroup", conversationId, userId);
        }

        private async Task SendMessageToGroup(int groupId, string SenderId, Message message, bool y, bool x)
        {
            await Clients.Group(groupId.ToString()).SendAsync("ReceiveMessage", SenderId, message, groupId);
        }

        private async Task SendMessageToUser(Message message, string SenderId, string UserToSendId, int conversationId, bool x, bool y)
        {
            await Clients.User(UserToSendId).SendAsync("ReceiveMessage", SenderId, message, conversationId);
        }
    }
}
