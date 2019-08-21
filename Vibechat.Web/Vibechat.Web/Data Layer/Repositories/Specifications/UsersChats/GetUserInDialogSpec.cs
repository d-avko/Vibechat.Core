using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.UsersChats
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
