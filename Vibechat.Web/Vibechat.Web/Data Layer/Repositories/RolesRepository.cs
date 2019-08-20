using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public class RolesRepository : IRolesRepository
    {
        private readonly ApplicationDbContext dbContext;

        public RolesRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public RoleDataModel Get(ChatRole role)
        {
            return dbContext.Roles.Find(role);
        }
    }
}
