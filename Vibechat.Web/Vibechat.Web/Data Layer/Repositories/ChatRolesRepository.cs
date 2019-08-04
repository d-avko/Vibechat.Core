using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VibeChat.Web;
using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Data.Repositories
{
    public class ChatRolesRepository : IChatRolesRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ChatRolesRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Add(int chatId, string userId, ChatRole role)
        {
            var newRecord = new ChatRoleDataModel
            {
                ChatId = chatId,
                UserId = userId,
                RoleId = role
            };

            dbContext.ChatRoles.Add(newRecord);
        }

        public Task<ChatRoleDataModel> GetAsync(int chatId, string userId)
        {
            return dbContext.ChatRoles.FirstOrDefaultAsync(x => x.UserId == userId && x.ChatId == chatId);
        }

        public Task<List<ChatRoleDataModel>> GetAsync(string userId)
        {
            return dbContext.ChatRoles.Where(x => x.UserId == userId).ToListAsync();
        }

        public void Remove(ChatRoleDataModel chatRole)
        {
            dbContext.Remove(chatRole);
        }

        public void Update(ChatRole role, ChatRoleDataModel chatRole)
        {
            var newRole = dbContext.Roles.Find(role);
            chatRole.Role = newRole;
        }
    }
}