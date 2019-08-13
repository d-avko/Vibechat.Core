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
        MessageDataModel Add(MessageDataModel message);
        bool Empty();
        IQueryable<MessageDataModel> GetByIds(List<int> ids);

        MessageDataModel GetById(int id);

        int GetUnreadAmount(int conversationId, string userId, int lastMessageId);

        void MarkAsRead(MessageDataModel message);

        IQueryable<MessageDataModel> Get(string userId, int conversationId, int maxMessageId,
            bool history = false,
            int offset = 0, int count = 0);

        IQueryable<MessageDataModel> GetAttachments(string userId, int conversationId,
            AttachmentKind attachmentKind, int offset, int count);
        IQueryable<MessageDataModel> Search
            (int offset, int count, string searchString, string userId);

        void Remove(List<int> messagesIds, string whoRemovedId);

        void RemovePermanent(List<MessageDataModel> messages);
    }
}