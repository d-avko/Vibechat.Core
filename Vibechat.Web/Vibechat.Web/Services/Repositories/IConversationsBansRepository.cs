using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IConversationsBansRepository
    {
        void BanUserInGroup(UserInApplication banned, ConversationDataModel where);

        void UnbanUserInGroup(string userId, int conversationId);

        bool IsBanned(UserInApplication who, int whereId);
    }
}