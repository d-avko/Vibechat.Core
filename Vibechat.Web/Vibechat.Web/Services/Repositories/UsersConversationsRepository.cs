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
        public UserInApplication GetUserInDialog(int convId, string FirstUserInDialogueId)
        {
            var UsersConvs = mContext.UsersConversations
               .Include(x => x.Conversation)
               .Include(y => y.User);

            return UsersConvs.Where(x => x.ChatID == convId && x.UserID != FirstUserInDialogueId)
                .FirstOrDefault()?
                .User;
        }

        public IQueryable<ConversationDataModel> GetUserConversations(string userId)
        {
            var usersConversations = mContext.UsersConversations
               .Include(x => x.Conversation)
               .Include(y => y.User);

            return from userConversation in usersConversations
                   where userConversation.UserID == userId
                   select userConversation.Conversation;
        }

        public IQueryable<UserInApplication> GetConversationParticipants(int conversationId)
        {
            //Update info from db
            var usersConversations = mContext.UsersConversations
                .Include(x => x.User)
                .Include(y => y.Conversation);

            return from userConversation in usersConversations
                   where userConversation.ChatID == conversationId
                   select userConversation.User;
        }

        public async Task<UsersConversationDataModel> Get(string userId, int conversationId)
        {
            return await mContext
                .UsersConversations
                .Include(x => x.User)
                .Include(x => x.Conversation)
                .FirstOrDefaultAsync(x => x.UserID == userId && x.ChatID == conversationId);
        }

        public async Task<bool> Exists(string userId, int conversationId)
        {
            return await mContext.UsersConversations
                .FirstOrDefaultAsync(x => x.ChatID == conversationId && x.UserID == userId) != default(UsersConversationDataModel);
        }

        public async Task Remove(UsersConversationDataModel entity)
        {
            mContext.UsersConversations.Remove(entity);
            await mContext.SaveChangesAsync();
        }

        public async Task<UsersConversationDataModel> Add(string userId, int chatId)
        {
            var res = await mContext.UsersConversations.AddAsync(new UsersConversationDataModel()
            {
                ChatID = chatId,
                UserID = userId
            });

            await mContext.SaveChangesAsync();

            return res.Entity;
        }

        public async Task<bool> Exists(UserInApplication user, ConversationDataModel conversation)
        {
            return await mContext
                .UsersConversations
                .FirstOrDefaultAsync(x => x.ChatID == conversation.Id && x.UserID == user.Id) != default(UsersConversationDataModel);
        }

        public async Task<bool> DialogExists(string firstUserId, string secondUserId, bool secure)
        {
            IQueryable<UsersConversationDataModel> firstUserConversations = mContext
               .UsersConversations
               .Where(x => x.UserID == firstUserId && !x.Conversation.IsGroup)
               .Include(x => x.Conversation)
               .Include(x => x.User);

            foreach(UsersConversationDataModel conversation in firstUserConversations)
            {
                if(await mContext
                    .UsersConversations
                    .AnyAsync(x => 
                    x.ChatID == conversation.ChatID
                    && x.User.Id == secondUserId
                    && x.Conversation.IsSecure == secure))
                {
                    return true;
                }
            }

            return false;
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
