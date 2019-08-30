using Vibechat.DataLayer.DataModels;
using Vibechat.Shared.DTO.Conversations;

namespace Vibechat.DataLayer.Repositories
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
