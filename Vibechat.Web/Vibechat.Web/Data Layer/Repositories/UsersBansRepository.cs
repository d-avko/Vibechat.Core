using System.Linq;
using VibeChat.Web;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Data.Repositories
{
    public class UsersBansRepository : IUsersBansRepository
    {
        public UsersBansRepository(ApplicationDbContext dbContext)
        {
            mContext = dbContext;
        }

        private ApplicationDbContext mContext { get; }

        public void BanUser(AppUser banned, AppUser bannedBy)
        {
            mContext.UsersBans.Add(new UsersBansDatamodel {BannedBy = bannedBy, BannedUser = banned});
        }

        public void UnbanUser(string userId, string whoUnbansId)
        {
            mContext.UsersBans.Remove(Get(userId, whoUnbansId));
        }

        public bool IsBanned(string whoId, string byWhomId)
        {
            return mContext.UsersBans.Any(x => x.BannedID == whoId && x.BannedByID == byWhomId);
        }

        public UsersBansDatamodel Get(string userId, string whoUnbansId)
        {
            return mContext.UsersBans.FirstOrDefault(x => x.BannedByID == whoUnbansId && x.BannedID == userId);
        }
    }
}