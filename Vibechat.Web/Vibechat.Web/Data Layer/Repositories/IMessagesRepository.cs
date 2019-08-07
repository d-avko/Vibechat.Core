using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using VibeChat.Web;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;
using Vibechat.Web.Data.Messages;

namespace Vibechat.Web.Data.Repositories
{
    public interface IMessagesRepository
    {
        MessageDataModel Add(AppUser whoSent, Message message, int groupId, MessageDataModel forwardedMessage);

        MessageDataModel AddAttachment(AppUser whoSent, MessageAttachmentDataModel attachment, Message message,
            int groupId);

        bool Empty();
        IQueryable<MessageDataModel> GetByIds(List<int> ids);

        MessageDataModel GetById(int id);

        int GetUnreadAmount(int conversationId, string userId, int lastMessageId);

        void MarkAsRead(MessageDataModel message);

        /// <summary>
        /// Returns messages for specified user and chat.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <param name="maxMessageId">messageId to start from</param>
        /// <param name="AllMessages"></param>
        /// <param name="offset"></param>
        /// <param name="count">amount to return.</param>
        /// <returns></returns>
        IIncludableQueryable<MessageDataModel, AppUser> Get(string userId, int conversationId, int maxMessageId,
            bool history = false,
            int offset = 0, int count = 0);

        IIncludableQueryable<MessageDataModel, AppUser> GetAttachments(string userId, int conversationId,
            AttachmentKind attachmentKind, int offset, int count);

        MessageDataModel AddSecureMessage(AppUser whoSent, string message, int groupId);

        /// <summary>
        /// Performs case-insensitive message search.
        /// </summary>
        /// <param name="chats"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="searchString"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IQueryable<MessageDataModel> Search
            (List<ConversationDataModel> chats, int offset, int count, string searchString, string userId);

        void Remove(List<int> messagesIds, string whoRemovedId);

        void RemovePermanent(List<MessageDataModel> messages);
    }
}