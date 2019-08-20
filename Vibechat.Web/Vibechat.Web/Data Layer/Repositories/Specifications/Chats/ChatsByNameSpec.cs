using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.Chats
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
