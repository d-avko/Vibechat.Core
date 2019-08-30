using Microsoft.EntityFrameworkCore;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.UsersChats
{
    public class FindUsersInChatSpec : BaseSpecification<UsersConversationDataModel>
    {
        public FindUsersInChatSpec(
            string username, int chatId) : base(
                msg => msg.ChatID == chatId && EF.Functions.Like(msg.User.UserName, username + "%"))
        {
            AddInclude(x => x.User);
        }
    }
}
