﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.Messages
{
    public class GetLatestMessagesSpec : BaseSpecification<MessageDataModel>
    {
        public GetLatestMessagesSpec(
            IQueryable<DeletedMessagesDataModel> deletedMessages,
            int conversationId,
            int offset,
            int count 
            ) : 
            base(msg => msg.ConversationID == conversationId
                    && !deletedMessages.Any(x => x.Message.MessageID == msg.MessageID))
        {
            ApplyOrderByDescending(x => x.TimeReceived);
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
