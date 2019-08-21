using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.Messages
{
    public class UnreadMessagesCountSpec : BaseSpecification<MessageDataModel>
    {
        public UnreadMessagesCountSpec(IQueryable<DeletedMessagesDataModel> deletedMessages,
            int chatId, int lastMessageId) : 
            base(msg => msg.ConversationID == chatId
                        && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID)
                        && msg.MessageID > lastMessageId)
        {
        }
    }
}
