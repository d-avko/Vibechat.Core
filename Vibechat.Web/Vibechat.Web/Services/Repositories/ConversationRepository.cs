using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
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
                .Where(x => x.Id == id)
                .Include(x => x.PublicKey)
                .Single();
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
