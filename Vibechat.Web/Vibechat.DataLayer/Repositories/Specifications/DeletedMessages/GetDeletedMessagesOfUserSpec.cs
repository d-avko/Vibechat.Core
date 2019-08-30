using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.DeletedMessages
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
