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
        Task<MessageDataModel> Add(UserInApplication whoSent, Message message, int groupId);
        Task<MessageDataModel> AddAttachment(UserInApplication whoSent, MessageAttachmentDataModel attachment, Message message, int groupId, string SenderId);
        bool Empty();
        IQueryable<MessageDataModel> GetByIds(List<int> ids);
        IIncludableQueryable<MessageDataModel, MessageAttachmentDataModel> Get(string userId, int conversationId, bool AllMessages = false, int offset = 0, int count = 0);
        IIncludableQueryable<MessageDataModel, MessageAttachmentDataModel> GetAttachments(string userId, int conversationId, string attachmentKind, int offset, int count);
        Task Remove(List<int> messagesIds, string whoRemovedId);
    }
}