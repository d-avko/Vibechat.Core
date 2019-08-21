using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.UsersChats
{
    public class GetUserChatsSpec : BaseSpecification<UsersConversationDataModel>
    {
        public GetUserChatsSpec(string deviceId, string userId) : 
            base(chat => chat.UserID == userId && //deviceId could be null because key exchange didn't finish
                        (chat.DeviceId == deviceId || chat.DeviceId == null))
        {
            AddNestedInclude(x => x.Conversation.PublicKey);
            AsNoTracking();
        }
    }
}
