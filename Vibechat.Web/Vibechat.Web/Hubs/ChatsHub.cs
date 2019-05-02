using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using VibeChat.Web.ChatData;
using VibeChat.Web.UserProviders;

namespace VibeChat.Web
{
    /// <summary>
    /// SignalR main hub
    /// </summary>
    public class ChatsHub : Hub
    {
        private ApplicationDbContext dbContext { get; set; }

        private ICustomHubUserIdProvider userProvider { get; set; }


        public ChatsHub(ApplicationDbContext dbContext, ICustomHubUserIdProvider userProvider)
        { 
            this.dbContext = dbContext;
            this.userProvider = userProvider;
        }

        public async Task OnConnected()
        {
            
            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == userProvider.GetUserId(Context));

            user.IsOnline = true;

            user.LastSeen = DateTime.UtcNow;

            user.ConnectionId = Context.ConnectionId;

            await dbContext.SaveChangesAsync();
        }

        public async Task OnDisconnected()
        {
            var user = await dbContext.Users
                .SingleOrDefaultAsync(u => u.Id == userProvider.GetUserId(Context));

            user.IsOnline = false;

            user.ConnectionId = null;

            dbContext.SaveChanges();         
        }

        /// <summary>
        /// Used to add user to specified group
        /// </summary>
        /// <param name="userId">id of a user</param>
        /// <param name="conversationId">conversation to add to</param>
        /// <param name="connectionId"> connection to add</param>
        /// <returns></returns>
        public async Task AddToGroup(string userId, int conversationId, string connectionId)
        {
            await Groups.AddToGroupAsync(connectionId, conversationId.ToString());

            try
            {
                await AddedToGroup(userId, conversationId, true);
            }
            catch(Exception ex)
            {
                await AddedToGroup(userId, conversationId, false, ex.Message);
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

        /// <summary>
        /// Used to stop the connection with a group
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task DeletedFromGroup(int groupId)
        {            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task SendMessageToGroup(int groupId, string SenderId, string message)
        {
            try
            {
                await SendMessageToGroup(groupId, SenderId, message, true);

                var whoSent = await dbContext.Users.FindAsync(SenderId).ConfigureAwait(false);

                if (whoSent == null)
                {
                    await SendMessageToGroup(groupId, null, null, false, $"Failed to retrieve user with id {SenderId} from database: no such user exists");
                    return;
                }

                dbContext.Messages.Add(new MessageDataModel()
                {
                    ConversationID = groupId,
                    MessageContent = message,
                    TimeReceived = DateTime.UtcNow,
                    User = whoSent
                });

                await dbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                await SendMessageToGroup(groupId, null, null, false, ex.Message);
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
        public async Task SendMessageToUser(string message, string SenderId, string UserToSendId, int conversationId)
        {
            try
            {
                await SendMessageToUser(message, SenderId, UserToSendId, conversationId, true);

                var whoSent = await dbContext.Users.FindAsync(SenderId).ConfigureAwait(false);

                if (whoSent == null)
                {
                    await SendMessageToUser(null, null, UserToSendId, 0, false, $"Failed to retrieve user with id {SenderId} from database: no such user exists");
                    return;
                }

                dbContext.Messages.Add(new MessageDataModel()
                {
                    User = whoSent,
                    ConversationID = conversationId,
                    MessageContent = message,
                    TimeReceived = DateTime.UtcNow
                });

                await dbContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                await SendMessageToUser(message, SenderId, UserToSendId, conversationId, true, ex.Message);
            }
        }

        private async Task AddedToGroup(string userId, int conversationId, bool IsSuccessfull, string ErrorMessage = null)
        {
            await Clients.Group(conversationId.ToString()).SendAsync("AddedToGroup", conversationId, userId, IsSuccessfull, ErrorMessage);
        }

        private async Task SendMessageToGroup(int groupId, string SenderId, string message, bool IsSuccessfull, string ErrorMessage = null)
        {
            await Clients.Group(groupId.ToString()).SendAsync("ReceiveMessage", SenderId, message, IsSuccessfull, groupId, ErrorMessage);
        }

        private async Task SendMessageToUser(string message, string SenderId, string UserToSendId, int conversationId, bool IsSuccessfull, string ErrorMessage = null)
        {
            await Clients.User(UserToSendId).SendAsync("ReceiveMessage", SenderId, message,IsSuccessfull, conversationId, ErrorMessage);
        }
    }
}
