using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IConversationsBansRepository
    {
        void BanUserInGroup(AppUser banned, ConversationDataModel where);

        void UnbanUserInGroup(string userId, int conversationId);

        bool IsBanned(AppUser who, int whereId);
    }
}