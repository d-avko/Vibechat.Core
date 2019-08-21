using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.DeletedMessages
{
    public class GetDeletedMessagesOfUserSpec : BaseSpecification<DeletedMessagesDataModel>
    {
        public GetDeletedMessagesOfUserSpec(string userId) :
            base(msg =>  msg.UserId == userId)
        {
            AsNoTracking();
        }
    }
}
