using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.UsersChats
{
    public class GetParticipantsSpec : BaseSpecification<UsersConversationDataModel>
    {
        public GetParticipantsSpec(int chatId) : 
            base(chat => chat.ChatID == chatId)
        {
            AddInclude(x => x.User);
            AsNoTracking();
        }
    }
}
