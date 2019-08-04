using System.Linq;
using VibeChat.Web;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Data.Repositories
{
    public class ConversationsBansRepository : IConversationsBansRepository
    {
        public ConversationsBansRepository(ApplicationDbContext dbContext)
        {
            mContext = dbContext;
        }

        private ApplicationDbContext mContext { get; }

        public void BanUserInGroup(AppUser banned, ConversationDataModel where)
        {
            mContext.ConversationsBans.Add(new ConversationsBansDataModel {BannedUser = banned, Conversation = where});
        }

        public void UnbanUserInGroup(string userId, int conversationId)
        {
            mContext.ConversationsBans.Remove(Get(userId, conversationId));
        }

        public bool IsBanned(AppUser who, int whereId)
        {
            return mContext.ConversationsBans.Any(x => x.ChatID == whereId && x.UserID == who.Id);
        }

        public ConversationsBansDataModel Get(string userId, int conversationId)
        {
            return mContext.ConversationsBans.FirstOrDefault(x => x.UserID == userId && x.ChatID == conversationId);
        }
    }
}