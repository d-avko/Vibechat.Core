using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using VibeChat.Web;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Services.Repositories
{
    public interface IMessagesRepository
    {
        Task<MessageDataModel> Add(AppUser whoSent, Message message, int groupId, MessageDataModel forwardedMessage);
        Task<MessageDataModel> AddAttachment(AppUser whoSent, MessageAttachmentDataModel attachment, Message message, int groupId);
        bool Empty();
        IQueryable<MessageDataModel> GetByIds(List<int> ids);

        MessageDataModel GetById(int id);

        int GetUnreadAmount(int conversationId, string userId);

        void MarkAsRead(MessageDataModel message);

        IIncludableQueryable<MessageDataModel, MessageAttachmentDataModel> Get(
            string userId, int conversationId, bool AllMessages = false, int offset = 0, int count = 0);
        IIncludableQueryable<MessageDataModel, MessageAttachmentDataModel> GetAttachments(
            string userId, int conversationId, string attachmentKind, int offset, int count);

        Task<MessageDataModel> AddSecureMessage(AppUser whoSent, string message, int groupId);

        Task Remove(List<int> messagesIds, string whoRemovedId);

        Task RemovePermanent(List<MessageDataModel> messages);
    }
}