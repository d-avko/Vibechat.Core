using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public class ChatRolesRepository : BaseRepository<ChatRoleDataModel>, IChatRolesRepository
    {
        public ChatRolesRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            
        }
        
        public Task<ChatRoleDataModel> GetByIdAsync(int chatId, string userId)
        {
            return _dbContext.ChatRoles.FindAsync(chatId, userId);
        }
    }
}