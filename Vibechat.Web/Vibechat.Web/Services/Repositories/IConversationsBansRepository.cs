using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IConversationsBansRepository
    {
        void BanUserInGroup(UserInApplication banned, ConversationDataModel where);

        bool IsBanned(UserInApplication who, int whereId);
    }
}