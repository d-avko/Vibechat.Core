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

            return UsersConvs.Where(x => x.Conversation.ConvID == convId && x.User.Id != FirstUserInDialogueId)
                .FirstOrDefault()?
                .User;
        }

        public IQueryable<ConversationDataModel> GetUserConversations(string userId)
        {
            var usersConversations = mContext.UsersConversations
               .Include(x => x.Conversation)
               .Include(y => y.User);

            return from userConversation in usersConversations
                   where userConversation.User.Id == userId
                   select userConversation.Conversation;
        }

        public IQueryable<UserInApplication> GetConversationParticipants(int conversationId)
        {
            //Update info from db
            var usersConversations = mContext.UsersConversations
                .Include(x => x.User)
                .Include(y => y.Conversation);

            return from userConversation in usersConversations
                   where userConversation.Conversation.ConvID == conversationId
                   select userConversation.User;
        }

        public async Task<UsersConversationDataModel> Get(string userId, int conversationId)
        {
            return await mContext
                .UsersConversations
                .Include(x => x.User)
                .Include(x => x.Conversation)
                .FirstOrDefaultAsync(x => x.User.Id == userId && x.Conversation.ConvID == conversationId);
        }

        public async Task<bool> Exists(string userId, int conversationId)
        {
            return await mContext.UsersConversations
                .FirstOrDefaultAsync(x => x.Conversation.ConvID == conversationId && x.User.Id == userId) != default(UsersConversationDataModel);
        }

        public async Task Remove(UsersConversationDataModel entity)
        {
            mContext.UsersConversations.Remove(entity);
            await mContext.SaveChangesAsync();
        }

        public async Task<UsersConversationDataModel> Add(UserInApplication user, ConversationDataModel conversation)
        {
            var res = await mContext.UsersConversations.AddAsync(new UsersConversationDataModel()
            {
                Conversation = conversation,
                User = user
            });

            await mContext.SaveChangesAsync();

            return res.Entity;
        }
    }
}
