using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Vibechat.Web.Data.Messages;
using Vibechat.Web.DTO.Messages;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.Messages
{
    public class GetSpecificAttachmentsSpec : BaseSpecification<MessageDataModel>
    {
        public GetSpecificAttachmentsSpec(
            IQueryable<DeletedMessagesDataModel> deletedMessages,
            int conversationId,
            AttachmentKind attachmentKind,
            int offset,
            int count
            ) : 
            base(
                msg =>
                msg.ConversationID == conversationId
                && msg.Type == MessageType.Attachment
                && msg.AttachmentInfo.AttachmentKind.Kind == attachmentKind
                && !deletedMessages.Any(x => x.MessageID == msg.MessageID)
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
