using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public class ConversationsBansRepository : IConversationsBansRepository
    {
        private ApplicationDbContext mContext { get; set; }

        public ConversationsBansRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public void BanUserInGroup(UserInApplication banned, ConversationDataModel where)
        {
            mContext.ConversationsBans.Add(new ConversationsBansDataModel() { BannedUser = banned, Conversation = where });
            mContext.SaveChanges();
        }

        public bool IsBanned(UserInApplication who, int whereId)
        {
            return mContext.ConversationsBans.Any(x => x.Conversation.ConvID == whereId && x.BannedUser.Id == who.Id);
        }
    }
}
