using System.Linq;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.Messages
{
    public class GetRecentMessagesSpec : BaseSpecification<MessageDataModel>
    {
        public GetRecentMessagesSpec(
            IQueryable<DeletedMessagesDataModel> deletedMessages,
            int conversationId,
            int maxMessageId,
            int offset,
            int count
            ) :
            base(msg => msg.ConversationID == conversationId
                    && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID)
                    && msg.MessageID >= maxMessageId)
        {
            ApplyOrderBy(x => x.TimeReceived);
            ApplyPaging(offset, count);
            AddNestedInclude(x => x.AttachmentInfo.AttachmentKind);
            AddInclude(x => x.User);
            AddNestedInclude(x => x.ForwardedMessage.AttachmentInfo.AttachmentKind);
            AddNestedInclude(x => x.ForwardedMessage.User);
            AddInclude(x => x.Event);
            AddInclude(x => x.Event.Actor);
            AddInclude(x => x.Event.UserInvolved);
            AsNoTracking();
        }
    }
}
