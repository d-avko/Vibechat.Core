using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.DeletedMessages
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
