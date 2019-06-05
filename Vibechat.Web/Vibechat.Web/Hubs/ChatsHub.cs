using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vibechat.Web.Extensions;
using Vibechat.Web.Services;
using Vibechat.Web.Services.Bans;
using Vibechat.Web.Services.Extension_methods;
using Vibechat.Web.Services.Users;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;
using VibeChat.Web.UserProviders;

namespace VibeChat.Web
{
    public class ChatsHub : Hub
    {
        private ICustomHubUserIdProvider userProvider { get; set; }

        private UsersInfoService userService { get; set; }

        private ConversationsInfoService conversationsService { get; set; }
        public BansService bansService { get; }
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
            this.bansService = bansService;
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
            await userService.MakeUserOnline(whoSentId, Context.ConnectionId);

            try
            {
                await conversationsService.RemoveUserFromConversation(userToRemoveId, whoSentId, conversationId);
                await RemovedFromGroup(userToRemoveId, conversationId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
            }
            catch(Exception ex)
            {
                await SendError(whoSentId, ex.Message);
                logger.LogError(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task AddToGroup(string userId, ConversationTemplate conversation)
        {
            UserInApplication whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                var isBanned = await bansService.IsBannedFromConversation(conversation.ConversationID, userId);

                if (isBanned)
                {
                    await SendError(whoSent.ConnectionId, "You were banned from this group. Couldn't join it.");
                    return;
                }

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
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                if (conversation.IsGroup)
                {
                    await RemovedFromGroup(conversation.DialogueUser.Id, conversation.ConversationID);
                    await RemovedFromGroup(whoSent.Id, conversation.ConversationID);
                }
                else
                {
                    List<UserInfo> participants = (await conversationsService
                        .GetParticipants(new GetParticipantsApiModel() { ConvId = conversation.ConversationID })).Participants;

                    foreach(UserInfo user in participants)
                    {
                        UserInApplication userToSend = await userService.GetUserById(user.Id).ConfigureAwait(false);

                        if (userToSend.IsOnline)
                        {
                            await RemovedFromDialog(user.Id, userToSend.ConnectionId, conversation.ConversationID);
                        }
                    }
                }

                await conversationsService.RemoveConversation(conversation, whoSent.Id);
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
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                if (bansService.IsBannedFromMessagingWith(whoSent.Id, user.Id))
                {
                    await SendError(whoSent.ConnectionId, "You were blocked by this user. Couldn't create dialog.");
                    return;
                }

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


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task ConnectToGroups(List<int> groupIds)
        {
            UserInApplication whoSent = await userService.GetUserById(userProvider.GetUserId(Context));

            foreach (var groupId in groupIds)
            {
                //establish connections only with groups where user exists.

                if (await conversationsService.ExistsInConversation(groupId, whoSent.Id))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
                }
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task SendMessageToGroup(Message message, int groupId)
        {
            UserInApplication whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                if(!await conversationsService.ExistsInConversation(groupId, whoSent.Id))
                {
                    await SendError(whoSent.ConnectionId, "You must join this group to send messages.");
                    return;
                }

                if(await bansService.IsBannedFromConversation(groupId , whoSent.Id))
                {
                    await SendError(whoSent.ConnectionId, "You were banned in this group.");
                    return;
                }

                //we can't trust user on what's in user field

                message.User = whoSent.ToUserInfo();
                
                var created = new MessageDataModel();

                if (message.IsAttachment)
                {
                    created = await conversationsService.AddAttachmentMessage(message, groupId, whoSent.Id);
                }
                else
                {
                    created = await conversationsService.AddMessage(message, groupId, whoSent.Id);
                }

                message.TimeReceived = created.TimeReceived.ToUTCString();
                message.Id = created.MessageID;

                await SendMessageToGroup(groupId, whoSent.Id, message, true, true);
            }
            catch(Exception ex)
            {
                await SendError(whoSent.ConnectionId, ex.Message);
                logger.LogError(ex.Message);
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task SendMessageToUser(Message message, string UserToSendId, int conversationId)
        {
            UserInApplication whoSent = await userService.GetUserById(userProvider.GetUserId(Context));
            await userService.MakeUserOnline(whoSent.Id, Context.ConnectionId);

            try
            {
                if (bansService.IsBannedFromMessagingWith(whoSent.Id, UserToSendId))
                {
                    await SendError(whoSent.ConnectionId, "You were blocked by this user. Couldn't send message.");
                    return;
                }

                //we can't trust user on what's in user field

                message.User = whoSent.ToUserInfo();

                var created = new MessageDataModel();

                if (message.IsAttachment)
                {
                    created = await conversationsService.AddAttachmentMessage(message, conversationId, whoSent.Id);
                }
                else
                {
                    created = await conversationsService.AddMessage(message, conversationId, whoSent.Id);
                }

                message.TimeReceived = created.TimeReceived.ToUTCString();
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

        private async Task SendError(string connectionId, string error)
        {
            await Clients.Client(connectionId).SendAsync("Error", error);
        }
    }
}
