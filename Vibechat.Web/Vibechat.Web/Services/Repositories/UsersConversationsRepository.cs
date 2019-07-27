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

        public AppUser GetUserInDialog(int convId, string FirstUserInDialogueId)
        {
            return mContext.UsersConversations
                .Where(x => x.ChatID == convId && x.UserID != FirstUserInDialogueId)
                .Include(x => x.User)
                .First()?
                .User;
        }

        public IQueryable<ConversationDataModel> GetUserConversations(string deviceId, string userId)
        {

            return (from userConversation in mContext.UsersConversations //deviceId could be null because key exchange didn't finish
                    where userConversation.UserID == userId && (userConversation.DeviceId == deviceId || userConversation.DeviceId == null)
                    select userConversation.Conversation
                   )
                   .Include(x => x.PublicKey);
        }

        public IQueryable<AppUser> GetConversationParticipants(int conversationId)
        {
            return from userConversation in mContext.UsersConversations
                   where userConversation.ChatID == conversationId
                   select userConversation.User;
        }

        public Task<List<AppUser>> FindUsersInChat(string username, int chatId)
        {
            return mContext.
                UsersConversations
                .Where(x => x.ChatID == chatId && EF.Functions.Like(x.User.UserName, username + "%"))
                .Select(x => x.User).ToListAsync();
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

        public void Remove(UsersConversationDataModel entity)
        {
            mContext.UsersConversations.Remove(entity);
        }

        public UsersConversationDataModel Add(string userId, int chatId, string deviceId = null)
        {
            return mContext.UsersConversations.Add(new UsersConversationDataModel()
            {
                ChatID = chatId,
                UserID = userId,
                DeviceId = deviceId
            })?.Entity;
        }

        public void UpdateDeviceId(string deviceId, string userId, int chatId)
        {
            var chat = mContext
                .UsersConversations
                .Where(x => x.UserID == userId && x.ChatID == chatId).Single();

            chat.DeviceId = deviceId;
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
