using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.UsersChats
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
