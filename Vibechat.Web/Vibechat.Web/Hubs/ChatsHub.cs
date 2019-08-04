using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;
using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Data.Repositories;
using Vibechat.Web.Extensions;
using Vibechat.Web.Services;
using Vibechat.Web.Services.Bans;
using Vibechat.Web.Services.Extension_methods;
using Vibechat.Web.Services.Users;
using VibeChat.Web.UserProviders;

namespace VibeChat.Web
{
    public class ChatsHub : Hub
    {
        public enum BlockEvent
        {
            Block = 0,
            Unblock = 1
        }

        private readonly UsersSubsriptionService subsriptionService;
        private readonly UnitOfWork unitOfWork;


        public ChatsHub(
            ICustomHubUserIdProvider userProvider,
            UsersService userService,
            ChatService chatsService,
            BansService bansService,
            ILogger<ChatsHub> logger,
            UsersSubsriptionService subsriptionService,
            UnitOfWork unitOfWork)
        {
            this.userProvider = userProvider;
            this.userService = userService;
            this.chatsService = chatsService;
            this.bansService = bansService;
            this.logger = logger;
            this.subsriptionService = subsriptionService;
            this.unitOfWork = unitOfWork;
        }

        private ICustomHubUserIdProvider userProvider { get; }

        private UsersService userService { get; }

        private ChatService chatsService { get; }
        public BansService bansService { get; }
        private ILogger<ChatsHub> logger { get; }

        public override async Task OnConnectedAsync()
        {
            await OnUserOnline();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await OnUserOffline();
            await base.OnDisconnectedAsync(ex);
        }

        public async Task OnUserOnline()
        {
            try
            {
                var userId = userProvider.GetUserId(Context);
                await userService.MakeUserOnline(userId, Context.ConnectionId);

                var subs = subsriptionService.GetSubscribers(userId);

                if (subs == null) return;

                foreach (var sub in subs) await NotifyOfUserOnline(userId, sub);
            }
            catch (Exception ex)
            {
                logger.LogError("Error while updating user state!", ex.Message);
            }
        }

        public async Task OnUserOffline()
        {
            try
            {
                await userService.MakeUserOffline(userProvider.GetUserId(Context));
            }
            catch (Exception ex)
            {
                logger.LogError("Error while updating user state!", ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task RemoveFromGroup(string userToRemoveId, int conversationId, bool IsSelf)
        {
            var whoSentId = userProvider.GetUserId(Context);
            await userService.MakeUserOnline(whoSentId, Context.ConnectionId);

            try
            {
                await chatsService.RemoveUserFromConversation(userToRemoveId, whoSentId, conversationId, IsSelf);
                await RemovedFromGroup(userToRemoveId, conversationId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        public async Task<bool> BlockUser(string userId, BlockEvent blockType)
        {
            var whoSentId = userProvider.GetUserId(Context);
            await userService.MakeUserOnline(whoSentId, Context.ConnectionId);

            try
            {
                switch (blockType)
                {
                    case BlockEvent.Block:
                    {
                        await bansService.BanDialog(userId, whoSentId);
                    }
                        break;
                    case BlockEvent.Unblock:
                    {
                        await bansService.UnbanDialog(
                            userId,
                            whoSentId);
                    }
                        break;
                }

                var user = await userService.GetUserById(userId);

                if (user.IsOnline) await SendUserBlocked(user.ConnectionId, whoSentId, blockType);

                return true;
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
                return false;
            }
        }

        public async Task<bool> BlockUserInChat(string userId, int chatId, BlockEvent blockType)
        {
            var whoSentId = userProvider.GetUserId(Context);
            await userService.MakeUserOnline(whoSentId, Context.ConnectionId);

            try
            {
                switch (blockType)
                {
                    case BlockEvent.Block:
                    {
                        await bansService.BanUserFromConversation(
                            chatId,
                            userId,
                            whoSentId);
                    }
                        break;
                    case BlockEvent.Unblock:
                    {
                        await bansService.UnbanUserFromConversation(
                            chatId,
                            userId,
                            whoSentId);
                    }
                        break;
                }

                await SendUserBlockedInChat(chatId, userId, blockType);
                return true;
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
                return false;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<bool> ChangeUserRole(string userId, int chatId, ChatRole newRole)
        {
            var whoSentId = userProvider.GetUserId(Context);
            await userService.MakeUserOnline(whoSentId, Context.ConnectionId);

            try
            {
                await chatsService.ChangeUserRole(chatId, userId, whoSentId, newRole);
                await SendUserRoleChanged(chatId, userId, newRole);
                return true;
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
                return false;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public bool SubscribeToUserOnlineStatusChanges(string userId)
        {
            try
            {
                var whoSentId = userProvider.GetUserId(Context);
                subsriptionService.AddSubsriber(userId, whoSentId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public bool UnsubscribeFromUserOnlineStatusChanges(string userId)
        {
            try
            {
                var whoSentId = userProvider.GetUserId(Context);
                subsriptionService.RemoveSubsriber(userId, whoSentId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<bool> AddToGroup(string userId, int chatId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                var isBanned = await bansService.IsBannedFromConversation(chatId, userId);

                if (isBanned)
                {
                    await SendError(Context.ConnectionId, "You were banned from this group. Couldn't join it.");
                    return false;
                }

                var addedUser = await chatsService.AddUserToConversation(chatId, userId);

                if (addedUser.IsOnline) await Groups.AddToGroupAsync(addedUser.ConnectionId, chatId.ToString());

                await AddedToGroup(addedUser, chatId, Context.ConnectionId, true);
                return true;
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
                return false;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task OnTyping(int chatId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            await SendTyping(whoSent.Id, whoSent.FirstName ?? whoSent.UserName, chatId);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task RemoveConversation(int chatId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                var chat = await chatsService.GetById(chatId, whoSent.Id);

                if (chat.IsGroup)
                {
                    var participants = await chatsService
                        .GetParticipants(chat.Id);

                    foreach (var user in participants)
                    {
                        var userToSend = await userService.GetUserById(user.Id);

                        if (userToSend.IsOnline) await RemovedFromGroup(user.Id, chat.Id);
                    }
                }
                else
                {
                    var userToSend = await userService.GetUserById(chat.DialogueUser.Id);

                    if (userToSend.IsOnline) await RemovedFromDialog(userToSend.Id, userToSend.ConnectionId, chat.Id);

                    await RemovedFromDialog(whoSent.Id, Context.ConnectionId, chat.Id);
                }

                await chatsService.RemoveConversation(chat, whoSent.Id);
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task CreateDialog(UserInfo user, bool secure, string deviceId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                if (bansService.IsBannedFromMessagingWith(whoSent.Id, user.Id))
                {
                    await SendError(Context.ConnectionId, "You were blocked by this user. Couldn't create dialog.");
                    return;
                }

                var created = await chatsService
                    .CreateConversation(new CreateConversationCredentialsApiModel
                    {
                        IsGroup = false,
                        DialogUserId = user.Id,
                        CreatorId = whoSent.Id,
                        IsSecure = secure,
                        DeviceId = deviceId
                    });

                var userToSend = await userService.GetUserById(user.Id);

                if (whoSent.IsOnline)
                    //send to self 
                    await AddedToDialog(new UserInfo {Id = whoSent.Id}, Context.ConnectionId, created.Id);

                if (userToSend.IsOnline)
                    await AddedToDialog(new UserInfo {Id = userToSend.Id}, userToSend.ConnectionId, created.Id);
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task ConnectToGroups(List<int> groupIds)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            foreach (var groupId in groupIds)
                //establish connections only with groups where user exists.

                if (await chatsService.ExistsInConversation(groupId, whoSent.Id))
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        /// <summary>
        ///     Marks message as read, updates client last message id.
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="conversationId"></param>
        /// <returns>true in case of success, false otherwise</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<bool> MessageRead(int msgId, int conversationId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            try
            {
                await chatsService.MarkMessageAsRead(msgId, conversationId, whoSent.Id);

                var conversation = await chatsService.GetByIdSimplified(conversationId, whoSent.Id);

                if (conversation.IsGroup)
                    await MessageReadInGroup(msgId, conversationId);
                else
                    await MessageReadInDialog(
                        conversation.DialogueUser.IsOnline ? conversation.DialogueUser.ConnectionId : null,
                        Context.ConnectionId, msgId, conversationId);

                return true;
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                return false;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<int> SendMessageToGroup(Message message, int groupId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                if (!await chatsService.ExistsInConversation(groupId, whoSent.Id))
                {
                    await SendError(Context.ConnectionId, "You must join this group to send messages.");
                    return 0;
                }

                if (await bansService.IsBannedFromConversation(groupId, whoSent.Id))
                {
                    await SendError(Context.ConnectionId, "You were banned in this group.");
                    return 0;
                }

                //we can't trust user on what's in user field

                message.User = whoSent.ToUserInfo();

                MessageDataModel created;

                if (message.IsAttachment)
                    created = await chatsService.AddAttachmentMessage(message, groupId, whoSent.Id);
                else
                    created = await chatsService.AddMessage(message, groupId, whoSent.Id);

                message.TimeReceived = created.TimeReceived.ToUTCString();
                message.Id = created.MessageID;
                message.State = MessageState.Delivered;

                await SendMessageToGroupExcept(groupId, Context.ConnectionId, whoSent.Id, message);
                return created.MessageID;
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
                return 0;
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<int> SendMessageToUser(Message message, string UserToSendId, int conversationId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                if (bansService.IsBannedFromMessagingWith(whoSent.Id, UserToSendId))
                {
                    await SendError(Context.ConnectionId, "You were blocked by this user. Couldn't send message.");
                    return 0;
                }

                //we can't trust user on what's in user field

                message.User = whoSent.ToUserInfo();

                MessageDataModel created;

                if (message.IsAttachment)
                    created = await chatsService.AddAttachmentMessage(message, conversationId, whoSent.Id);
                else
                    created = await chatsService.AddMessage(message, conversationId, whoSent.Id);

                message.TimeReceived = created.TimeReceived.ToUTCString();
                message.Id = created.MessageID;
                message.State = MessageState.Delivered;

                var userToSend = await userService.GetUserById(UserToSendId);

                if (userToSend.IsOnline)
                    await SendMessageToUser(message, whoSent.Id, userToSend.ConnectionId, conversationId);

                return created.MessageID;
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException)
                    await SendError(Context.ConnectionId, "Wrong user id was provided.");
                else
                    await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
                return 0;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<int> SendSecureMessage(string encryptedMessage, string userId, int conversationId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                await chatsService.ValidateDialog(whoSent.Id, userId, conversationId);
                var user = await userService.GetUserById(userId);
                var created = await chatsService.AddEncryptedMessage(encryptedMessage, conversationId, whoSent.Id);

                var toSend = new Message
                {
                    Id = created.MessageID,
                    EncryptedPayload = created.EncryptedPayload,
                    TimeReceived = created.TimeReceived.ToUTCString(),
                    State = MessageState.Delivered,
                    //this is really needed, because if name/lastname of sender will change, it won't be reflected in encrypted payload.
                    User = whoSent.ToUserInfo()
                };

                if (user.IsOnline) await SendMessageToUser(toSend, whoSent.Id, user.ConnectionId, conversationId, true);

                return toSend.Id;
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException)
                    await SendError(Context.ConnectionId, "Wrong user id was provided.");
                else
                    await SendError(Context.ConnectionId, ex.Message);

                logger.LogError(ex.Message);
                return 0;
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task SendDhParam(string userId, string param, int chatId)
        {
            var thisUserId = userProvider.GetUserId(Context);

            try
            {
                var user = await userService.GetUserById(userId);

                if (user.IsOnline) await SendDhParamTo(user.ConnectionId, param, thisUserId, chatId);
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException)
                    await SendError(Context.ConnectionId, "Wrong user id was provided.");
                else
                    await SendError(Context.ConnectionId, ex.Message);

                logger.LogError(ex.Message);
            }
        }

        private async Task NotifyOfUserOnline(string userId, string whereTo)
        {
            var user = await userService.GetUserById(whereTo);

            if (user.IsOnline) await Clients.Client(user.ConnectionId).SendAsync("UserOnline", userId);
        }

        private Task SendUserRoleChanged(int chatId, string userId, ChatRole newRole)
        {
            return Clients.Group(chatId.ToString()).SendAsync("UserRoleChanged", userId, chatId, newRole);
        }

        private Task SendUserBlocked(string connectionId, string blockedBy, BlockEvent blockType)
        {
            return Clients.Client(connectionId).SendAsync("Blocked", blockedBy, blockType);
        }

        private Task SendUserBlockedInChat(int chatId, string userId, BlockEvent blockType)
        {
            return Clients.Group(chatId.ToString()).SendAsync("BlockedInChat", chatId, userId, blockType);
        }

        private Task SendTyping(string userId, string userFirstName, int chatId)
        {
            return Clients.Group(chatId.ToString()).SendAsync("Typing", userId, userFirstName, chatId);
        }

        private Task SendDhParamTo(string connectionId, string param, string sentBy, int chatId)
        {
            return Clients.Client(connectionId).SendAsync("ReceiveDhParam", param, sentBy, chatId);
        }

        private Task RemovedFromGroup(string userId, int conversationId)
        {
            return Clients.Group(conversationId.ToString()).SendAsync("RemovedFromGroup", userId, conversationId);
        }

        private Task AddedToGroup(UserInfo user, int chatId, string callerConnectionId, bool x)
        {
            return Clients.GroupExcept(chatId.ToString(), callerConnectionId).SendAsync("AddedToGroup", chatId, user);
        }

        private Task AddedToDialog(UserInfo user, string connectionId, int conversationId)
        {
            return Clients.Client(connectionId).SendAsync("AddedToGroup", conversationId, user);
        }

        private Task RemovedFromDialog(string userId, string connectionId, int conversationId)
        {
            return Clients.Client(connectionId).SendAsync("RemovedFromGroup", userId, conversationId);
        }

        private Task SendMessageToGroupExcept(int groupId, string exceptConnectionId, string SenderId, Message message,
            bool secure = false)
        {
            return Clients.GroupExcept(groupId.ToString(), exceptConnectionId)
                .SendAsync("ReceiveMessage", SenderId, message, groupId, secure);
        }

        private Task SendMessageToUser(Message message, string SenderId, string UserToSendConnectionId,
            int conversationId, bool secure = false)
        {
            return Clients.Client(UserToSendConnectionId)
                .SendAsync("ReceiveMessage", SenderId, message, conversationId, secure);
        }

        private Task MessageReadInGroup(int messageId, int conversationId)
        {
            return Clients.Group(conversationId.ToString()).SendAsync("MessageRead", messageId, conversationId);
        }

        private Task MessageReadInDialog(string dialogUserConnectionId, string SenderConnectionId, int messageId,
            int conversationId)
        {
            if (dialogUserConnectionId != null)
                return Clients.Client(dialogUserConnectionId).SendAsync("MessageRead", messageId, conversationId);

            return Task.CompletedTask;
        }

        private Task SendError(string connectionId, string error)
        {
            return Clients.Client(connectionId).SendAsync("Error", error);
        }
    }
}