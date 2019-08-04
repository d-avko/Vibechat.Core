using System.Linq;
using VibeChat.Web;
using Vibechat.Web.Data_Layer.DataModels;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public class LastMessagesRepository : ILastMessagesRepository
    {
        public LastMessagesRepository(ApplicationDbContext dbContext)
        {
            mContext = dbContext;
        }

        private ApplicationDbContext mContext { get; }

        public void Update(LastMessageDataModel entry)
        {
            mContext.LastViewedMessages.Update(entry);
        }

        public void Add(string userId, int chatId, int msgId)
        {
            mContext.LastViewedMessages.Add(new LastMessageDataModel
            {
                ChatID = chatId,
                UserID = userId,
                MessageID = msgId
            });
        }

        public LastMessageDataModel Get(string userId, int chatId)
        {
            return mContext.LastViewedMessages.SingleOrDefault(
                entry => entry.ChatID == chatId && entry.UserID == userId);
        }

        public void Remove(LastMessageDataModel entry)
        {
            mContext.LastViewedMessages.Remove(entry);
        }
    }
}