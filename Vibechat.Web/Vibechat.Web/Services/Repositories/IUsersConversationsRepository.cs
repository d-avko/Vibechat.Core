using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IUsersConversationsRepository
    {
        Task<UsersConversationDataModel> Add(UserInApplication user, ConversationDataModel conversation);
        Task<UsersConversationDataModel> Get(string userId, int conversationId);
        IQueryable<UserInApplication> GetConversationParticipants(int conversationId);
        IQueryable<ConversationDataModel> GetUserConversations(string userId);
        UserInApplication GetUserInDialog(int convId, string FirstUserInDialogueId);

        Task<bool> Exists(string userId, int conversationId);
    }
}