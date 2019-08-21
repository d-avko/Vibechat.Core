using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.UsersChats
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
