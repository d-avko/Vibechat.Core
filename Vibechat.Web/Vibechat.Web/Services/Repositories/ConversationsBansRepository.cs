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

        public ConversationsBansDataModel Get(string userId, int conversationId)
        {
            return mContext.ConversationsBans.FirstOrDefault(x => x.UserID == userId && x.ChatID == conversationId);
        }

        public void BanUserInGroup(AppUser banned, ConversationDataModel where)
        {
            mContext.ConversationsBans.Add(new ConversationsBansDataModel() { BannedUser = banned, Conversation = where });
        }

        public void UnbanUserInGroup(string userId, int conversationId)
        {
            mContext.ConversationsBans.Remove(Get(userId, conversationId));
        }

        public bool IsBanned(AppUser who, int whereId)
        {
            return mContext.ConversationsBans.Any(x => x.ChatID == whereId && x.UserID == who.Id);
        }
    }
}
