using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.DeletedMessages
{
    public class GetDeletedMessagesSpec : BaseSpecification<DeletedMessagesDataModel>
    {
        public GetDeletedMessagesSpec(int chatId, string userId) : 
            base(msg => msg.Message.ConversationID == chatId && msg.UserId == userId)
        {
            AsNoTracking();
        }
    }
}
