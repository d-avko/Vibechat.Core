using System;
using System.Linq.Expressions;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.Messages
{
    public class GetMostRecentMessageSpec : BaseSpecification<MessageDataModel>
    {
        public GetMostRecentMessageSpec(int chatId) 
            : base(message => message.ConversationID == chatId)
        {
            ApplyOrderBy(x => x.TimeReceived);
            ApplyPaging(0, 1);
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