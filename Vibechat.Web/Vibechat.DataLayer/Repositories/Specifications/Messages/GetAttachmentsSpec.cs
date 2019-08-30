using System.Linq;
using Vibechat.DataLayer.DataModels;
using Vibechat.Shared.DTO.Messages;

namespace Vibechat.DataLayer.Repositories.Specifications.Messages
{
    public class GetAttachmentsSpec: BaseSpecification<MessageDataModel>
    {
        public GetAttachmentsSpec(
            IQueryable<DeletedMessagesDataModel> deletedMessages,
            int conversationId,
            int offset,
            int count
        ) : 
            base(
                msg =>
                    msg.ConversationID == conversationId
                    && msg.Type == MessageType.Attachment
                    && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID)
            )
        {
            ApplyOrderByDescending(x => x.TimeReceived);
            ApplyPaging(offset, count);
            AddNestedInclude(x => x.AttachmentInfo.AttachmentKind);
            AddInclude(x => x.User);
            AddNestedInclude(x => x.ForwardedMessage.AttachmentInfo.AttachmentKind);
            AddNestedInclude(x => x.ForwardedMessage.User);
            AsNoTracking();
        }
    }
}