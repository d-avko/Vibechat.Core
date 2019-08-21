using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories.Specifications.UsersChats
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
