using System.Linq;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.Messages
{
    public class UnreadMessagesCountSpec : BaseSpecification<MessageDataModel>
    {
        public UnreadMessagesCountSpec(string userId,
            int chatId, int lastMessageId) :  
            base(msg => msg.ConversationID == chatId
                        && msg.DeletedEntries.All(x => x.UserId != userId)
                        && msg.MessageID > lastMessageId)
        {
        }
    }
}
