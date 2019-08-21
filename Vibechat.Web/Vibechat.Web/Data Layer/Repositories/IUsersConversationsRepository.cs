using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data_Layer.Repositories;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public interface IUsersConversationsRepository : IAsyncRepository<UsersConversationDataModel>
    {
        ValueTask<UsersConversationDataModel> GetByIdAsync(string userId, int conversationId);
        Task<AppUser> GetUserInDialog(int convId, string FirstUserInDialogueId);

        Task<bool> Exists(string userId, int conversationId);

        Task<UsersConversationDataModel> GetDialog(string firstUserId, string secondUserId);

        Task<IEnumerable<ConversationDataModel>> GetUserChats(string deviceId, string userId);

        Task<IEnumerable<AppUser>> GetChatParticipants(int chatId);

        Task<IEnumerable<AppUser>> FindUsersInChat(int chatId, string username);
    }
}