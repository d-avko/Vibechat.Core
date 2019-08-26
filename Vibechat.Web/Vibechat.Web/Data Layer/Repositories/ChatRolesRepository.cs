using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VibeChat.Web;
using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Data.DataModels;
using Vibechat.Web.Data_Layer.Repositories;
using Vibechat.Web.Data_Layer.Repositories.Specifications;

namespace Vibechat.Web.Data.Repositories
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