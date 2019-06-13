using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public class UsersSessionsRepository : IUsersSessionsRepository
    {
        private ApplicationDbContext mContext { get; set; }

        public UsersSessionsRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public IQueryable<SessionDataModel> GetSessions(UserInApplication user)
        {
            return mContext.UsersSessions
                 .Where(x => x.User.Id == user.Id)
                 .Select(x => x.Session);
        }
    }
}
