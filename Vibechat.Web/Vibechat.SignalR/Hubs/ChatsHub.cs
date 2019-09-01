using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Vibechat.BusinessLogic.Extensions;
using Vibechat.BusinessLogic.Services.Bans;
using Vibechat.BusinessLogic.Services.Chat;
using Vibechat.BusinessLogic.Services.Messages;
using Vibechat.BusinessLogic.Services.Users;
using Vibechat.BusinessLogic.UserProviders;
using Vibechat.DataLayer.DataModels;
using Vibechat.Shared.ApiModels.Conversation;
using Vibechat.Shared.DTO.Conversations;
using Vibechat.Shared.DTO.Messages;
using Vibechat.Shared.DTO.Users;

namespace Vibechat.SignalR.Hubs
{
    public class ChatsHub : ChatHubBase
    {
        public enum BlockEvent
        {
            Block = 0,
            Unblock = 1
        }

        private readonly UsersSubscriptionService subscriptionService;
        private readonly MessagesService messagesService;


        public ChatsHub(
            ICustomHubUserIdProvider userProvider,
            UsersService userService,
            ChatService chatsService,
            BansService bansService,
            ILogger<ChatsHub> logger,
            UsersSubscriptionService subscriptionService,
            MessagesService messagesService)
        {
            this.userProvider = userProvider;
            this.userService = userService;
            this.chatsService = chatsService;
            this.bansService = bansService;
            this.logger = logger;
            this.subscriptionService = subscriptionService;
            this.messagesService = messagesService;
        }

        private ICustomHubUserIdProvider userProvider { get; }

        private UsersService userService { get; }

        private ChatService chatsService { get; }
        private BansService bansService { get; }
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

        private async Task OnUserOnline()
        {
            try
            {
                var userId = userProvider.GetUserId(Context);
                await userService.MakeUserOnline(userId, Context.ConnectionId);

                var subs = subscriptionService.GetSubscribers(userId);

                if (subs == null)
                {
                    return;
                }
                
                async Task NotifyOfUserOnline(string userID, string whereTo)
                {
                    var user = await userService.GetUserById(whereTo);

                    if (user.IsOnline && user.ConnectionId != null)
                    {
                        await SendUserIsOnline(user.ConnectionId, userID);
                    }
                }

                foreach (var sub in subs)
                {
                    await NotifyOfUserOnline(userId, sub);
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Error while updating user state!", ex.Message);
            }
        }

        private async Task OnUserOffline()
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

        [Authorize(Policy = "PublicApi")]
        public async Task RemoveFromGroup(string userToRemoveId, int conversationId, bool IsSelf)
        {
            var whoSentId = userProvider.GetUserId(Context);
            await userService.MakeUserOnline(whoSentId, Context.ConnectionId);

            try
            {
                await chatsService.RemoveUserFromConversation(userToRemoveId, whoSentId, conversationId, IsSelf);
                MessageDataModel chatEvent;

                if (IsSelf || userToRemoveId == whoSentId)
                {
                    chatEvent = await messagesService.AddChatEvent(ChatEventType.Left, userToRemoveId,
                        userToRemoveId, conversationId);
                }
                else
                {
                    chatEvent = await messagesService.AddChatEvent(ChatEventType.Kicked, whoSentId,
                        userToRemoveId, conversationId);
                }

                var removedUser = await userService.GetUserById(userToRemoveId);

               
                await RemovedFromGroup(userToRemoveId, conversationId);
                
                if (removedUser.IsOnline && removedUser.ConnectionId != null)
                {
                    await RemoveConnectionFromGroup(removedUser.ConnectionId, conversationId);
                }
                
                await SendMessageToGroup(conversationId, whoSentId, chatEvent.ToMessage());
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

                if (user.IsOnline && user.ConnectionId != null)
                {
                    await SendUserBlocked(user.ConnectionId, whoSentId, blockType);
                }

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

                        var chatEvent = await messagesService.AddChatEvent(ChatEventType.Banned,
                            whoSentId, userId, chatId);

                        await SendMessageToGroup(chatId, whoSentId, chatEvent.ToMessage());
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

        [Authorize(Policy = "PublicApi")]
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

        [Authorize(Policy = "PublicApi")]
        public bool SubscribeToUserOnlineStatusChanges(string userId)
        {
            try
            {
                var whoSentId = userProvider.GetUserId(Context);
                subscriptionService.AddSubscriber(userId, whoSentId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Authorize(Policy = "PublicApi")]
        public bool UnsubscribeFromUserOnlineStatusChanges(string userId)
        {
            try
            {
                var whoSentId = userProvider.GetUserId(Context);
                subscriptionService.RemoveSubscriber(userId, whoSentId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Authorize(Policy = "PublicApi")]
        public async Task<bool> AddToGroup(string userId, int chatId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                var isBanned = await bansService.IsBannedFromConversation(chatId, userId);

                //user can't join, but can be invited.
                if (isBanned && userId == whoSent.Id)
                {
                    await SendError(Context.ConnectionId, "You were banned from this group. Couldn't join it.");
                    return false;
                }

                var addedUser = await chatsService.AddUserToChat(chatId, userId);

                if (addedUser.IsOnline && addedUser.ConnectionId != null)
                {
                    await AddConnectionToGroup(addedUser.ConnectionId, chatId);
                }

                MessageDataModel chatEvent;
                if (userId == whoSent.Id)
                {
                    chatEvent = await messagesService.AddChatEvent(ChatEventType.Joined, userId,
                        null, chatId);
                }
                else
                {
                    chatEvent = await messagesService.AddChatEvent(ChatEventType.Invited, whoSent.Id,
                        userId, chatId);
                }

                await AddedToGroup(addedUser, chatId, Context.ConnectionId, true);
                
                await SendMessageToGroup(chatId, whoSent.Id, chatEvent.ToMessage());
                return true;
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
                return false;
            }
        }

        [Authorize(Policy = "PublicApi")]
        public async Task OnTyping(int chatId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            await SendTyping(whoSent.Id, whoSent.FirstName ?? whoSent.UserName, chatId);
        }

        [Authorize(Policy = "PublicApi")]
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

                        if (userToSend.IsOnline && userToSend.ConnectionId != null)
                        {
                            await RemovedFromGroup(user.Id, chat.Id);
                        }
                    }
                }
                else
                {
                    var userToSend = await userService.GetUserById(chat.DialogueUser.Id);

                    if (userToSend.IsOnline && userToSend.ConnectionId != null)
                    {
                        await RemovedFromDialog(userToSend.Id, userToSend.ConnectionId, chat.Id);
                    }

                    await RemovedFromDialog(whoSent.Id, Context.ConnectionId, chat.Id);
                }

                await chatsService.RemoveChat(chat, whoSent.Id);
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        [Authorize(Policy = "PublicApi")]
        public async Task CreateDialog(AppUserDto user, bool secure, string deviceId)
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
                    .CreateConversation(new CreateChatRequest
                    {
                        IsGroup = false,
                        DialogUserId = user.Id,
                        CreatorId = whoSent.Id,
                        IsSecure = secure,
                        DeviceId = deviceId
                    });

                var userToSend = await userService.GetUserById(user.Id);

                if (whoSent.IsOnline && whoSent.ConnectionId != null)
                {
                    await AddedToDialog(new AppUserDto {Id = whoSent.Id}, Context.ConnectionId, created.Id);
                }

                if (userToSend.IsOnline && userToSend.ConnectionId != null)
                {
                    await AddedToDialog(new AppUserDto {Id = userToSend.Id}, userToSend.ConnectionId, created.Id);
                }
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }


        [Authorize(Policy = "PublicApi")]
        public async Task ConnectToGroups(List<int> groupIds)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            foreach (var groupId in groupIds)
            //establish connections only with groups where user exists.
            {
                if (await chatsService.ExistsInConversation(groupId, whoSent.Id))
                {
                    await AddConnectionToGroup(Context.ConnectionId, groupId);
                }
            }
        }

        /// <summary>
        ///     Marks message as read, updates client last message id.
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="conversationId"></param>
        /// <returns>true in case of success, false otherwise</returns>
        [Authorize(Policy = "PublicApi")]
        public async Task<bool> MessageRead(int msgId, int conversationId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            try
            {
                var conversation = await chatsService.GetByIdSimplified(conversationId, whoSent.Id);

                if (conversation.IsGroup)
                {
                    return false;
                }

                await messagesService.MarkMessageAsRead(msgId, conversationId, whoSent.Id);

                await MessageReadInDialog(
                    conversation.DialogueUser.IsOnline ? conversation.DialogueUser.ConnectionId : null, msgId,
                    conversationId);

                return true;
            }
            catch (Exception ex)
            {
                await SendError(Context.ConnectionId, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sends message to specified group and stores it.
        /// </summary>
        /// <param name="encryptedMessage"></param>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        [Authorize(Policy = "PublicApi")]
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

                message.User = whoSent.ToAppUserDto();

                MessageDataModel created;

                if (message.Type == MessageType.Attachment)
                {
                    created = await messagesService.AddAttachmentMessage(message, groupId, whoSent.Id);
                }
                else
                {
                    created = await messagesService.AddMessage(message, groupId, whoSent.Id);
                }

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


        /// <summary>
        /// Sends message to specified user and stores it.
        /// </summary>
        /// <param name="encryptedMessage"></param>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        [Authorize(Policy = "PublicApi")]
        public async Task<int> SendMessageToUser(Message message, string userToSendId, int conversationId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                if (bansService.IsBannedFromMessagingWith(whoSent.Id, userToSendId))
                {
                    await SendError(Context.ConnectionId, "You were blocked by this user. Couldn't send message.");
                    return 0;
                }

                //we can't trust user on what's in user field

                message.User = whoSent.ToAppUserDto();

                MessageDataModel created;

                if (message.Type == MessageType.Attachment)
                {
                    created = await messagesService.AddAttachmentMessage(message, conversationId, whoSent.Id);
                }
                else
                {
                    created = await messagesService.AddMessage(message, conversationId, whoSent.Id);
                }

                message.TimeReceived = created.TimeReceived.ToUTCString();
                message.Id = created.MessageID;
                message.State = MessageState.Delivered;

                var userToSend = await userService.GetUserById(userToSendId);

                if (userToSend.IsOnline && userToSend.ConnectionId != null)
                {
                    await SendMessageToUser(message, whoSent.Id, userToSend.ConnectionId, conversationId);
                }

                return created.MessageID;
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
                return 0;
            }
        }

        /// <summary>
        /// Stores encrypted message and sends it to recipient.
        /// </summary>
        /// <param name="encryptedMessage"></param>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        [Authorize(Policy = "PublicApi")]
        public async Task<int> SendSecureMessage(string encryptedMessage, string userId, int conversationId)
        {
            var whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                await chatsService.ValidateDialog(whoSent.Id, userId);
                var user = await userService.GetUserById(userId);
                var created = await messagesService.AddEncryptedMessage(encryptedMessage, conversationId, whoSent.Id);

                var toSend = new Message
                {
                    Id = created.MessageID,
                    EncryptedPayload = created.EncryptedPayload,
                    TimeReceived = created.TimeReceived.ToUTCString(),
                    State = MessageState.Delivered,
                    //this is really needed, because if name/lastname of sender will change,
                    //it won't be reflected in encrypted payload.
                    User = whoSent.ToAppUserDto()
                };

                if (user.IsOnline && user.ConnectionId != null)
                {
                    await SendMessageToUser(toSend, whoSent.Id, user.ConnectionId, conversationId, true);
                }

                return toSend.Id;
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
                return 0;
            }
        }

        [Authorize(Policy = "PublicApi")]
        public async Task SendDhParam(string userId, string param, int chatId)
        {
            var thisUserId = userProvider.GetUserId(Context);

            try
            {
                var user = await userService.GetUserById(userId);

                if (user.IsOnline && user.ConnectionId != null)
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

    }
}