using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data_Layer.DataModels;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public class LastMessagesRepository : ILastMessagesRepository
    {
        private ApplicationDbContext mContext { get; set; }

        public LastMessagesRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public void Update(LastMessageDataModel entry)
        {
            mContext.LastViewedMessages.Update(entry);
        }

        public void Add(string userId, int chatId, int msgId)
        {
            mContext.LastViewedMessages.Add(new LastMessageDataModel()
            {
                ChatID = chatId,
                UserID = userId,
                MessageID = msgId
            });
        }

        public LastMessageDataModel Get(string userId, int chatId)
        {
            return mContext.LastViewedMessages.SingleOrDefault(entry => entry.ChatID == chatId && entry.UserID == userId);
        }

        public void Remove(LastMessageDataModel entry)
        {
            mContext.LastViewedMessages.Remove(entry);
        }
    }
}
