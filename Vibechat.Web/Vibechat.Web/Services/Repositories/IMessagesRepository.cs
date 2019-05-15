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
        IQueryable<MessageDataModel> GetMessagesByIds(List<int> ids);
        IIncludableQueryable<MessageDataModel, MessageAttachmentDataModel> GetMessagesForConversation(string userId, int conversationId, int offset, int count);
        Task Remove(List<int> messagesIds, string whoRemovedId);
    }
}