using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.UsersChats
{
    public class GetUserInDialogSpec : BaseSpecification<UsersConversationDataModel>
    {
        public GetUserInDialogSpec(int chatId, string firstUserInDialog) :
            base(chat => chat.ChatID == chatId && chat.UserID != firstUserInDialog)
        {
            AddInclude(x => x.User);
        }

    }
}
