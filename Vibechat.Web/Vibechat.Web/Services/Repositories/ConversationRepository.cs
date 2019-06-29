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
                .Include(x => x.Creator)
                .FirstOrDefault(x => x.Id == id);
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

        public async Task<IQueryable<ConversationDataModel>> SearchByName(string name, UserInApplication whoSearches, IUsersConversationsRepository participantsProvider)
        {
            var result = mContext
                .Conversations
                .Where(conv => conv.IsPublic)
                .Where(conv => conv.Name.StartsWith(name, StringComparison.InvariantCultureIgnoreCase));

            var finalResult = new List<ConversationDataModel>();

            foreach(var conversation in result)
            {
                // search only for conversations where 'whoSearches' doesn't exist.
                if (participantsProvider
                .GetConversationParticipants(conversation.Id)
                .FirstOrDefault(x => x.Id == whoSearches.Id) == default(UserInApplication))
                {
                    finalResult.Add(conversation);
                }
            }

            return finalResult.AsQueryable();
        }

        public async Task ChangePublicState(int conversationId)
        {
            var conversation = GetById(conversationId);
            conversation.IsPublic = !conversation.IsPublic;
            await mContext.SaveChangesAsync();
        }

        public void Remove(ConversationDataModel entity)
        {
            mContext.Conversations.Remove(entity);
            mContext.SaveChanges();
        }

        public async Task Add(ConversationDataModel conversation)
        {
            await mContext.Conversations.AddAsync(conversation);

            await mContext.SaveChangesAsync();
        }

        public async Task UpdateAuthKey(ConversationDataModel chat, string authKeyId)
        {
            chat.AuthKeyId = authKeyId;
            await mContext.SaveChangesAsync();
        }
    }
}
