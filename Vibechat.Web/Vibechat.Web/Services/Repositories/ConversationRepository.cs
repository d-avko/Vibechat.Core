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

        public async Task<IQueryable<ConversationDataModel>> SearchByName(string name)
        {
            return mContext
                .Conversations
                .Where(conv => conv.IsPublic)
                .Where(conv => conv.Name.StartsWith(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Remove(ConversationDataModel entity)
        {
            mContext.Conversations.Remove(entity);
            mContext.SaveChanges();
        }

        public async Task<ConversationDataModel> Add(
            bool IsGroup,
            string name,
            string imageUrl,
            UserInApplication creator,
            bool IsPublic)
        {
            var ConversationToAdd = new ConversationDataModel()
            {
                IsGroup = IsGroup,
                Name = name,
                FullImageUrl = imageUrl,
                ThumbnailUrl = imageUrl,
                Creator = creator,
                IsPublic = IsPublic
            };

            await mContext.Conversations.AddAsync(ConversationToAdd);

            await mContext.SaveChangesAsync();

            return ConversationToAdd;
        }
    }
}
