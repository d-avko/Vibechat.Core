using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public class UsersConversationsRepository : IUsersConversationsRepository
    {
        private ApplicationDbContext mContext { get; set; }

        public UsersConversationsRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        /// <summary>
        /// Helper method used to find user with whom 
        /// current user have a dialogue
        /// </summary>
        /// <param name="convId"></param>
        /// <param name="FirstUserInDialogueId"></param>
        /// <returns></returns>
        public AppUser GetUserInDialog(int convId, string FirstUserInDialogueId)
        {

            return mContext.UsersConversations.Where(x => x.ChatID == convId && x.UserID != FirstUserInDialogueId)
                .FirstOrDefault()?
                .User;
        }

        public IQueryable<ConversationDataModel> GetUserConversations(string userId)
        {

            return (from userConversation in mContext.UsersConversations
                    where userConversation.UserID == userId
                   select userConversation.Conversation
                   )
                   .Include(x => x.PublicKey)
                   .Include(x => x.Creator);
        }

        public IQueryable<AppUser> GetConversationParticipants(int conversationId)
        {
            return from userConversation in mContext.UsersConversations
                   where userConversation.ChatID == conversationId
                   select userConversation.User;
        }

        public async Task<UsersConversationDataModel> Get(string userId, int conversationId)
        {
            return await mContext
                .UsersConversations
                .Where(x => x.UserID == userId && x.ChatID == conversationId)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> Exists(string userId, int conversationId)
        {
            return await mContext.UsersConversations
                .SingleOrDefaultAsync(x => x.ChatID == conversationId && x.UserID == userId) != default(UsersConversationDataModel);
        }

        public async Task Remove(UsersConversationDataModel entity)
        {
            mContext.UsersConversations.Remove(entity);
            await mContext.SaveChangesAsync();
        }

        public async Task<UsersConversationDataModel> Add(string userId, int chatId, string deviceId = null)
        {
            var res = await mContext.UsersConversations.AddAsync(new UsersConversationDataModel()
            {
                ChatID = chatId,
                UserID = userId,
                DeviceId = deviceId
            });

            await mContext.SaveChangesAsync();

            return res.Entity;
        }

        public async Task<bool> Exists(AppUser user, ConversationDataModel conversation)
        {
            return await mContext
                .UsersConversations
                .FirstOrDefaultAsync(x => x.ChatID == conversation.Id && x.UserID == user.Id) != default(UsersConversationDataModel);
        }


        public async Task<UsersConversationDataModel> GetDialog(string firstUserId, string secondUserId)
        {
            IQueryable<UsersConversationDataModel> firstUserConversations = mContext
              .UsersConversations
              .Where(x => x.UserID == firstUserId && !x.Conversation.IsGroup)
              .Include(x => x.Conversation)
              .Include(x => x.User);

            foreach (UsersConversationDataModel conversation in firstUserConversations)
            {
                if (await mContext
                    .UsersConversations
                    .AnyAsync(x => x.ChatID == conversation.ChatID && x.UserID == secondUserId))
                {
                    return conversation;
                }
            }

            return null;
        }
    }
}
