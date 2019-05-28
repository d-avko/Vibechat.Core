using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public class UsersBansRepository : IUsersBansRepository
    {
        private ApplicationDbContext mContext { get; set; }

        public UsersBansRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public void BanUser(UserInApplication banned, UserInApplication bannedBy)
        {
            mContext.UsersBans.Add(new UsersBansDatamodel() { BannedBy = bannedBy, BannedUser = banned });
            mContext.SaveChanges();
        }

        public void UnbanUser(string userId, string whoUnbansId)
        {          
            mContext.UsersBans.Remove(Get(userId, whoUnbansId));
            mContext.SaveChanges();
        }

        public UsersBansDatamodel Get(string userId, string whoUnbansId)
        {
            return mContext.UsersBans.FirstOrDefault(x => x.BannedBy.Id == whoUnbansId && x.BannedUser.Id == userId);
        }

        public bool IsBanned(string whoId, string byWhomId)
        {
            return mContext.UsersBans.Any(x => x.BannedUser.Id == whoId && x.BannedBy.Id == byWhomId);
        }
    }
}
