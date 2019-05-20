using Microsoft.EntityFrameworkCore;
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
            return mContext
                .Conversations
                .Include(x => x.Creator)
                .FirstOrDefault(x => x.ConvID == id);
        }

        public void UpdateThumbnail(string thumbnailUrl, string fullimageUrl, ConversationDataModel entity)
        {
            entity.ThumbnailUrl = thumbnailUrl;
            entity.FullImageUrl = fullimageUrl;
            mContext.SaveChanges();
        }

        public void ChangeName(ConversationDataModel entity, string name)
        {
            entity.Name = name;
            mContext.SaveChanges();
        }

        public void Remove(ConversationDataModel entity)
        {
            mContext.Conversations.Remove(entity);
            mContext.SaveChanges();
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
                FullImageUrl = imageUrl,
                ThumbnailUrl = imageUrl
            };

            await mContext.Conversations.AddAsync(ConversationToAdd);

            await mContext.SaveChangesAsync();

            return ConversationToAdd;
        }
    }
}
