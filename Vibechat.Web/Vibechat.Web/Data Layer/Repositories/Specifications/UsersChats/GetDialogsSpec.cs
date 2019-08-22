using System;
using System.Linq.Expressions;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.UsersChats
{
    public class GetDialogsSpec : BaseSpecification<UsersConversationDataModel>
    {
        public GetDialogsSpec(string userId) : 
            base(entry => entry.UserID == userId && !entry.Conversation.IsGroup)
        {
            AddInclude(x => x.Conversation);
        }
    }
}