using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IUsersConversationsRepository
    {
        Task<UsersConversationDataModel> Add(string userId, int chatId);
        Task<UsersConversationDataModel> Get(string userId, int conversationId);
        IQueryable<AppUser> GetConversationParticipants(int conversationId);
        IQueryable<ConversationDataModel> GetUserConversations(string userId);
        AppUser GetUserInDialog(int convId, string FirstUserInDialogueId);

        Task<bool> Exists(string userId, int conversationId);

        Task Remove(UsersConversationDataModel entity);

        Task<bool> DialogExists(string firstUserId, string secondUserId, bool secure);

        Task<UsersConversationDataModel> GetDialog(string firstUserId, string secondUserId);
    }
}