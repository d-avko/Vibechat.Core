using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IUsersBansRepository
    {
        void BanUser(UserInApplication banned, UserInApplication bannedBy);
        bool IsBanned(string whoId, string byWhomId);
    }
}