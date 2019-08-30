using System.Collections.Generic;
using System.Linq;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.Messages
{
    public class MessagesByIdsSpec : BaseSpecification<MessageDataModel>
    {
        public MessagesByIdsSpec(List<int> ids) : base(
            msg => ids.Any(id => id == msg.MessageID))
        {
        }
    }
}
