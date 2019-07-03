using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenSSL.Crypto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vibechat.Web.Extensions;
using Vibechat.Web.Services;
using Vibechat.Web.Services.Bans;
using Vibechat.Web.Services.Extension_methods;
using Vibechat.Web.Services.Repositories;
using Vibechat.Web.Services.Users;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;
using VibeChat.Web.UserProviders;

namespace VibeChat.Web
{
    public class ChatsHub : Hub
    {
        private readonly UsersSubsriptionService subsriptionService;

        private ICustomHubUserIdProvider userProvider { get; set; }

        private UsersService userService { get; set; }

        private ChatService chatsService { get; set; }
        public BansService bansService { get; }
        private ILogger<ChatsHub> logger { get; set; }


        public ChatsHub(
            ICustomHubUserIdProvider userProvider, 
            UsersService userService,
            ChatService chatsService,
            BansService bansService,
            ILogger<ChatsHub> logger,
            UsersSubsriptionService subsriptionService)
        { 
            this.userProvider = userProvider;
            this.userService = userService;
            this.chatsService = chatsService;
            this.bansService = bansService;
            this.logger = logger;
            this.subsriptionService = subsriptionService;
        }

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
            var userId = userProvider.GetUserId(Context);
            await userService.MakeUserOnline(userId, Context.ConnectionId);

            var subs = subsriptionService.GetSubscribers(userId);

            if(subs == null)
            {
                return;
            }

            foreach (string sub in subs)
            {
                await NotifyOfUserOnline(userId, sub);
            }
        }

        public async Task OnUserOffline()
        {
            await userService.MakeUserOffline(userProvider.GetUserId(Context));
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
            catch(Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<bool> SubsribeToUserOnlineStatusChanges(string userId)
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
        public async Task<bool> UnsubsribeFromUserOnlineStatusChanges(string userId)
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
        public async Task AddToGroup(string userId, ConversationTemplate conversation)
        {
            AppUser whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                var isBanned = await bansService.IsBannedFromConversation(conversation.ConversationID, userId);

                if (isBanned)
                {
                    await SendError(Context.ConnectionId, "You were banned from this group. Couldn't join it.");
                    return;
                }

                var addedUser = await chatsService.AddUserToConversation(new AddToConversationApiModel()
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
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task RemoveConversation(ConversationTemplate conversation)
        {
            AppUser whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                if (conversation.IsGroup)
                {
                    List<UserInfo> participants = await chatsService
                        .GetParticipants(new GetParticipantsApiModel() { ConvId = conversation.ConversationID });

                    foreach (UserInfo user in participants)
                    {
                        AppUser userToSend = await userService.GetUserById(user.Id);

                        if (userToSend.IsOnline)
                        {
                            await RemovedFromGroup(user.Id, conversation.ConversationID);
                        }
                    }
                }
                else
                {
                    AppUser userToSend = await userService.GetUserById(conversation.DialogueUser.Id);

                    if (userToSend.IsOnline)
                    {
                        await RemovedFromDialog(userToSend.Id, userToSend.ConnectionId, conversation.ConversationID);
                    }

                    await RemovedFromDialog(whoSent.Id, Context.ConnectionId, conversation.ConversationID);
                }

                await chatsService.RemoveConversation(conversation, whoSent.Id);
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task CreateDialog(UserInfo user, bool secure)
        {
            AppUser whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                if (bansService.IsBannedFromMessagingWith(whoSent.Id, user.Id))
                {
                    await SendError(Context.ConnectionId, "You were blocked by this user. Couldn't create dialog.");
                    return;
                }

                ConversationTemplate created = await chatsService
                    .CreateConversation(new CreateConversationCredentialsApiModel()
                    {
                        IsGroup = false,
                        DialogUserId = user.Id,
                        CreatorId = whoSent.Id,
                        IsSecure = secure
                    });

                AppUser userToSend = await userService.GetUserById(user.Id);

                if (whoSent.IsOnline)
                {
                    created.DialogueUser = userToSend.ToUserInfo();
                    //send to self 
                    await AddedToDialog(new UserInfo() { Id = whoSent.Id }, Context.ConnectionId, created);
                }

                if (userToSend.IsOnline)
                {
                    created.DialogueUser = whoSent.ToUserInfo();
                    await AddedToDialog(new UserInfo() { Id = userToSend.Id }, userToSend.ConnectionId, created);
                }
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
            AppUser whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            foreach (var groupId in groupIds)
            {
                //establish connections only with groups where user exists.

                if (await chatsService.ExistsInConversation(groupId, whoSent.Id))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
                }
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task MessageRead(int msgId, int conversationId)
        {
            AppUser whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            try
            {
                await chatsService.MarkMessageAsRead(msgId, conversationId, whoSent.Id);

                ConversationTemplate conversation = await chatsService.GetByIdSimplified(conversationId, whoSent.Id);

                if (conversation.IsGroup)
                {
                    await MessageReadInGroup(msgId, conversationId);
                }
                else
                {
                    await MessageReadInDialog(conversation.DialogueUser.IsOnline ? conversation.DialogueUser.ConnectionId : null, Context.ConnectionId, msgId, conversationId);
                }
            }
            catch(Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task SendMessageToGroup(Message message, int groupId)
        {
            AppUser whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                if(!await chatsService.ExistsInConversation(groupId, whoSent.Id))
                {
                    await SendError(Context.ConnectionId, "You must join this group to send messages.");
                    return;
                }

                if(await bansService.IsBannedFromConversation(groupId , whoSent.Id))
                {
                    await SendError(Context.ConnectionId, "You were banned in this group.");
                    return;
                }

                //we can't trust user on what's in user field

                message.User = whoSent.ToUserInfo();
                
                var created = new MessageDataModel();

                if (message.IsAttachment)
                {
                    created = await chatsService.AddAttachmentMessage(message, groupId, whoSent.Id);
                }
                else
                {
                    created = await chatsService.AddMessage(message, groupId, whoSent.Id);
                }

                message.TimeReceived = created.TimeReceived.ToUTCString();
                int clientMessageId = message.Id;
                message.Id = created.MessageID;
                message.State = MessageState.Delivered;

                await SendMessageToGroupExcept(groupId, Context.ConnectionId, whoSent.Id, message);
                await MessageDelivered(Context.ConnectionId, message.Id, clientMessageId, groupId);
            }
            catch(Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task SendMessageToUser(Message message, string UserToSendId, int conversationId)
        {
            AppUser whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                if (bansService.IsBannedFromMessagingWith(whoSent.Id, UserToSendId))
                {
                    await SendError(Context.ConnectionId, "You were blocked by this user. Couldn't send message.");
                    return;
                }

                //we can't trust user on what's in user field

                message.User = whoSent.ToUserInfo();

                var created = new MessageDataModel();

                if (message.IsAttachment)
                {
                    created = await chatsService.AddAttachmentMessage(message, conversationId, whoSent.Id);
                }
                else
                {
                    created = await chatsService.AddMessage(message, conversationId, whoSent.Id);
                }

                message.TimeReceived = created.TimeReceived.ToUTCString();
                int clientMessageId = message.Id;
                message.Id = created.MessageID;
                message.State = MessageState.Delivered;

                var userToSend = await userService.GetUserById(UserToSendId);

                if (userToSend.IsOnline)
                {
                    await SendMessageToUser(message, whoSent.Id, userToSend.ConnectionId, conversationId, false);
                }

                await MessageDelivered(Context.ConnectionId, message.Id, clientMessageId, conversationId);
            }
            catch(Exception ex)
            {
                if (ex is NullReferenceException)
                {
                    await SendError(Context.ConnectionId, "Wrong user id was provided.");
                }
                else
                {
                    await SendError(Context.ConnectionId, ex.Message);
                }
                logger.LogError(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task SendSecureMessage(string encryptedMessage, int generatedMessageId, string userId, int conversationId)
        {
            AppUser whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                await chatsService.ValidateDialog(whoSent.Id, userId, conversationId);
                var user = await userService.GetUserById(userId);
                MessageDataModel created = await chatsService.AddEncryptedMessage(encryptedMessage, conversationId, whoSent.Id);

                var toSend = new Message()
                {
                    Id = created.MessageID,
                    EncryptedPayload = created.EncryptedPayload,
                    TimeReceived = created.TimeReceived.ToUTCString(),
                    State = MessageState.Delivered
                };

                if (user.IsOnline)
                {
                    await SendMessageToUser(toSend, whoSent.Id, user.ConnectionId, conversationId, true);
                }

                await MessageDelivered(Context.ConnectionId, toSend.Id, generatedMessageId, conversationId);
            }
            catch (Exception ex)
            {
                if(ex is NullReferenceException)
                {
                    await SendError(Context.ConnectionId, "Wrong user id was provided.");
                }
                else
                {
                    await SendError(Context.ConnectionId, ex.Message);
                }

                logger.LogError(ex.Message);
            }
           
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task SendDhParam(string userId, string param, int chatId)
        {
            var thisUserId = userProvider.GetUserId(Context);

            try
            {
                var user = await userService.GetUserById(userId);

                if (user.IsOnline)
                {
                    await SendDhParamTo(user.ConnectionId, param, thisUserId, chatId);
                }
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException)
                {
                    await SendError(Context.ConnectionId, "Wrong user id was provided.");
                }
                else
                {
                    await SendError(Context.ConnectionId, ex.Message);
                }

                logger.LogError(ex.Message);
            }
        }

        private async Task NotifyOfUserOnline(string userId, string whereTo)
        {
            var user = await userService.GetUserById(whereTo);

            if (user.IsOnline)
            {
                await Clients.Client(user.ConnectionId).SendAsync("UserOnline", userId);
            }
        }

        private async Task SendDhParamTo(string connectionId, string param, string sentBy, int chatId)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveDhParam", param, sentBy, chatId);
        }

        private async Task RemovedFromGroup(string userId, int conversationId)
        {
            await Clients.Group(conversationId.ToString()).SendAsync("RemovedFromGroup", userId, conversationId);
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

        private async Task SendMessageToGroupExcept(int groupId, string exceptConnectionId, string SenderId, Message message, bool secure = false)
        {
            await Clients.GroupExcept(groupId.ToString(), exceptConnectionId).SendAsync("ReceiveMessage", SenderId, message, groupId, secure);
        }

        private async Task SendMessageToUser(Message message, string SenderId, string UserToSendConnectionId, int conversationId, bool secure = false)
        {
            await Clients.Client(UserToSendConnectionId).SendAsync("ReceiveMessage", SenderId, message, conversationId, secure);
        }

        private async Task MessageDelivered(string connectionId, int messageId, int clientMessageId, int conversationId)
        {
            await Clients.Client(connectionId).SendAsync("MessageDelivered", messageId, clientMessageId, conversationId);
        }

        private async Task MessageReadInGroup(int messageId, int conversationId)
        {
            await Clients.Group(conversationId.ToString()).SendAsync("MessageRead", messageId, conversationId);
        }

        private async Task MessageReadInDialog(string dialogUserConnectionId, string SenderConnectionId, int messageId, int conversationId)
        {
            if(dialogUserConnectionId != null)
            {
                await Clients.Client(dialogUserConnectionId).SendAsync("MessageRead", messageId, conversationId);
            }

            await Clients.Client(SenderConnectionId).SendAsync("MessageRead", messageId, conversationId);
        }

        private async Task SendError(string connectionId, string error)
        {
            await Clients.Client(connectionId).SendAsync("Error", error);
        }
    }
}
