using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public interface IUsersBansRepository
    {
        void BanUser(AppUser banned, AppUser bannedBy);
        bool IsBanned(string whoId, string byWhomId);

        void UnbanUser(string userId, string whoUnbansId);
    }
}