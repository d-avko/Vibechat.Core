using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public class UsersBansRepository : IUsersBansRepository
    {
        private ApplicationDbContext mContext { get; set; }

        public UsersBansRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public void BanUser(AppUser banned, AppUser bannedBy)
        {
            mContext.UsersBans.Add(new UsersBansDatamodel() { BannedBy = bannedBy, BannedUser = banned });
        }

        public void UnbanUser(string userId, string whoUnbansId)
        {          
            mContext.UsersBans.Remove(Get(userId, whoUnbansId));
        }

        public UsersBansDatamodel Get(string userId, string whoUnbansId)
        {
            return mContext.UsersBans.FirstOrDefault(x => x.BannedByID == whoUnbansId && x.BannedID == userId);
        }

        public bool IsBanned(string whoId, string byWhomId)
        {
            return mContext.UsersBans.Any(x => x.BannedID == whoId && x.BannedByID == byWhomId);
        }
    }
}
