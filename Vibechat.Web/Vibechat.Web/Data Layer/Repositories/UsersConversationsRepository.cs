using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public class UsersConversationsRepository : IUsersConversationsRepository
    {
        public UsersConversationsRepository(ApplicationDbContext dbContext)
        {
            mContext = dbContext;
        }

        private ApplicationDbContext mContext { get; }

        public AppUser GetUserInDialog(int convId, string FirstUserInDialogueId)
        {
            return Queryable.First(mContext.UsersConversations
                    .Where(x => x.ChatID == convId && x.UserID != FirstUserInDialogueId)
                    .Include(x => x.User))?
                .User;
        }

        public IQueryable<ConversationDataModel> GetUserConversations(string deviceId, string userId)
        {
            return (from userConversation in
                        mContext.UsersConversations //deviceId could be null because key exchange didn't finish
                    where userConversation.UserID == userId &&
                          (userConversation.DeviceId == deviceId || userConversation.DeviceId == null)
                    select userConversation.Conversation
                )
                .Include(x => x.PublicKey)
                .AsNoTracking();
        }

        public IQueryable<AppUser> GetConversationParticipants(int conversationId)
        {
            return (from userConversation in mContext.UsersConversations
                where userConversation.ChatID == conversationId
                select userConversation.User).AsNoTracking();
        }

        public int GetParticipantsCount(int chatId)
        {
            return mContext.UsersConversations.Count(chat => chat.ChatID == chatId);
        }

        public Task<List<AppUser>> FindUsersInChat(string username, int chatId)
        {
            return mContext.UsersConversations
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
                       .SingleOrDefaultAsync(x => x.ChatID == conversationId && x.UserID == userId) !=
                   default(UsersConversationDataModel);
        }

        public void Remove(UsersConversationDataModel entity)
        {
            mContext.UsersConversations.Remove(entity);
        }

        public UsersConversationDataModel Add(string userId, int chatId, string deviceId = null)
        {
            return mContext.UsersConversations.Add(new UsersConversationDataModel
            {
                ChatID = chatId,
                UserID = userId,
                DeviceId = deviceId
            })?.Entity;
        }

        public UsersConversationDataModel Add(string userId, ConversationDataModel chat, string deviceId = null)
        {
            return mContext.UsersConversations.Add(new UsersConversationDataModel
            {
                Conversation = chat,
                UserID = userId,
                DeviceId = deviceId
            })?.Entity;
        }
        
        public void UpdateDeviceId(string deviceId, string userId, int chatId)
        {
            var chat = mContext
                .UsersConversations.Single(x => x.UserID == userId && x.ChatID == chatId);

            chat.DeviceId = deviceId;
        }


        public async Task<UsersConversationDataModel> GetDialog(string firstUserId, string secondUserId)
        {
            IQueryable<UsersConversationDataModel> firstUserConversations = mContext
                .UsersConversations
                .Where(x => x.UserID == firstUserId && !x.Conversation.IsGroup)
                .Include(x => x.Conversation)
                .Include(x => x.User);

            foreach (var conversation in firstUserConversations)
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