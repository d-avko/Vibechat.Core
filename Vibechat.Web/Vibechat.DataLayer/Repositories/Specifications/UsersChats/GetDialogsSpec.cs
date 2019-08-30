using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.UsersChats
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