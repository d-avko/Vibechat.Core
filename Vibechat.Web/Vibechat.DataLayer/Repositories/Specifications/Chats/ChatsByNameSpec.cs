using Microsoft.EntityFrameworkCore;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.Chats
{
    public class ChatsByNameSpec : BaseSpecification<ConversationDataModel>
    {
        public ChatsByNameSpec(string name) : base(
            chat => chat.IsPublic &&
            EF.Functions.Like(chat.Name, name + "%"))
        {
        }
    }
}
