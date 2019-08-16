using System.Linq;
using Microsoft.EntityFrameworkCore;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public class ConversationsRepository : IConversationRepository
    {
        public ConversationsRepository(ApplicationDbContext dbContext)
        {
            mContext = dbContext;
        }

        private ApplicationDbContext mContext { get; }

        public ConversationDataModel GetById(int id)
        {
            return mContext
                .Conversations
                .Single(x => x.Id == id);
        }

        public void UpdateThumbnail(string thumbnailUrl, string fullimageUrl, ConversationDataModel entity)
        {
            entity.ThumbnailUrl = thumbnailUrl;
            entity.FullImageUrl = fullimageUrl;
        }

        public void ChangeName(ConversationDataModel entity, string name)
        {
            entity.Name = name;
        }

        public IQueryable<ConversationDataModel> SearchByName(string name)
        {
            return mContext
                .Conversations
                .Where(conv => conv.IsPublic)
                .Where(conv => EF.Functions.Like(conv.Name, name + "%"));
        }

        public void ChangePublicState(ConversationDataModel chat)
        {
            chat.IsPublic = !chat.IsPublic;
        }

        public void Remove(ConversationDataModel entity)
        {
            mContext.Conversations.Remove(entity);
        }

        public void Add(ConversationDataModel conversation)
        {
            mContext.Conversations.Add(conversation);
        }

        public void UpdateAuthKey(ConversationDataModel chat, string authKeyId)
        {
            chat.AuthKeyId = authKeyId;
        }
    }
}