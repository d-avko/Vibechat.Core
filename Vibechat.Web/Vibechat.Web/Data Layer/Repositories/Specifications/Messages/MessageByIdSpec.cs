using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.Messages
{
    public class MessagesByIdsSpec : BaseSpecification<MessageDataModel>
    {
        public MessagesByIdsSpec(List<int> ids) : base(
            msg => ids.Any(id => id == msg.MessageID))
        {
        }
    }
}
