using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public class ConversationsRepository : IConversationRepository
    {
        private ApplicationDbContext mContext { get; set; }
        public ConversationsRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public ConversationDataModel GetById(int id)
        {
            return mContext.Conversations.FirstOrDefault(x => x.ConvID == id);
        }

        public async Task<ConversationDataModel> Add(
            bool IsGroup,
            string name,
            string imageUrl)
        {
            var ConversationToAdd = new ConversationDataModel()
            {
                IsGroup = IsGroup,
                Name = name,
                ImageUrl = imageUrl
            };

            await mContext.Conversations.AddAsync(ConversationToAdd);

            await mContext.SaveChangesAsync();

            return ConversationToAdd;
        }
    }
}
