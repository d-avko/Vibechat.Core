﻿using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IUsersConversationsRepository
    {
        Task<UsersConversationDataModel> Add(string userId, int chatId, string deviceId = null);
        Task<UsersConversationDataModel> Get(string userId, int conversationId);
        IQueryable<AppUser> GetConversationParticipants(int conversationId);
        IQueryable<ConversationDataModel> GetUserConversations(string deviceId, string userId);
        AppUser GetUserInDialog(int convId, string FirstUserInDialogueId);

        Task<bool> Exists(string userId, int conversationId);

        void UpdateDeviceId(string deviceId, string userId, int chatId);

        Task Remove(UsersConversationDataModel entity);

        Task<UsersConversationDataModel> GetDialog(string firstUserId, string secondUserId);
    }
}