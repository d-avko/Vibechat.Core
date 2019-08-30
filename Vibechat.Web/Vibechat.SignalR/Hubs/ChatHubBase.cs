using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Vibechat.Shared.DTO.Conversations;
using Vibechat.Shared.DTO.Messages;
using Vibechat.Shared.DTO.Users;

namespace Vibechat.SignalR.Hubs
{
    public class ChatHubBase: Hub
    {
        protected Task SendUserRoleChanged(int chatId, string userId, ChatRole newRole)
        {
            return Clients.Group(chatId.ToString()).SendAsync("UserRoleChanged", userId, chatId, newRole);
        }

        protected Task SendUserBlocked(string connectionId, string blockedBy, ChatsHub.BlockEvent blockType)
        {
            return Clients.Client(connectionId).SendAsync("Blocked", blockedBy, blockType);
        }

        protected Task SendUserBlockedInChat(int chatId, string userId, ChatsHub.BlockEvent blockType)
        {
            return Clients.Group(chatId.ToString()).SendAsync("BlockedInChat", chatId, userId, blockType);
        }

        protected Task SendTyping(string userId, string userFirstName, int chatId)
        {
            return Clients.Group(chatId.ToString()).SendAsync("Typing", userId, userFirstName, chatId);
        }

        protected  Task SendDhParamTo(string connectionId, string param, string sentBy, int chatId)
        {
            return Clients.Client(connectionId).SendAsync("ReceiveDhParam", param, sentBy, chatId);
        }

        protected  Task RemovedFromGroup(string userId, int conversationId)
        {
            return Clients.Group(conversationId.ToString()).SendAsync("RemovedFromGroup", userId, conversationId);
        }

        protected  Task AddedToGroup(AppUserDto user, int chatId, string callerConnectionId, bool x)
        {
            return Clients.GroupExcept(chatId.ToString(), callerConnectionId).SendAsync("AddedToGroup", chatId, user);
        }

        protected  Task AddedToDialog(AppUserDto user, string connectionId, int conversationId)
        {
            return Clients.Client(connectionId).SendAsync("AddedToGroup", conversationId, user);
        }

        protected  Task RemovedFromDialog(string userId, string connectionId, int conversationId)
        {
            return Clients.Client(connectionId).SendAsync("RemovedFromGroup", userId, conversationId);
        }

        protected  Task SendMessageToGroupExcept(int groupId, string exceptConnectionId, string senderId, Message message,
            bool secure = false)
        {
            return Clients.GroupExcept(groupId.ToString(), exceptConnectionId)
                .SendAsync("ReceiveMessage", senderId, message, groupId, secure);
        }

        protected  Task SendMessageToGroup(int groupId, string senderId, Message message,
            bool secure = false)
        {
            return Clients.Group(groupId.ToString())
                .SendAsync("ReceiveMessage", senderId, message, groupId, secure);
        }

        protected  Task SendMessageToUser(Message message, string senderId, string userToSendConnectionId,
            int conversationId, bool secure = false)
        {
            return Clients.Client(userToSendConnectionId)
                .SendAsync("ReceiveMessage", senderId, message, conversationId, secure);
        }

        protected  Task MessageReadInDialog(string dialogUserConnectionId, int messageId,
            int conversationId)
        {
            if (dialogUserConnectionId != null)
            {
                return Clients.Client(dialogUserConnectionId).SendAsync("MessageRead", messageId, conversationId);
            }

            return Task.CompletedTask;
        }
        
        protected Task SendUserIsOnline(string connectionId, string userId)
        {
            return Clients.Client(connectionId).SendAsync("UserOnline", userId);
        }

        protected Task SendError(string connectionId, string error)
        {
            return Clients.Client(connectionId).SendAsync("Error", error);
        }

        protected Task AddConnectionToGroup(string connectionId, int chatId)
        {
            return Groups.AddToGroupAsync(connectionId, chatId.ToString());
        }
        
        protected Task RemoveConnectionFromGroup(string connectionId, int chatId)
        {
            return Groups.RemoveFromGroupAsync(connectionId, chatId.ToString());
        }
        
    }
}