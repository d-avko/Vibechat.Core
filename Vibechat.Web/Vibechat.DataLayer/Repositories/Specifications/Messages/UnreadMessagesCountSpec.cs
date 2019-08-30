using System.Linq;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.Messages
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
