using System.Linq;
using VibeChat.Web;
using Vibechat.Web.Data.DataModels;
using Vibechat.Web.Data_Layer.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Vibechat.Web.Data.Repositories
{
    public class UsersBansRepository : BaseRepository<UsersBansDatamodel>, IUsersBansRepository
    {
        public UsersBansRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            
        }

        public async Task<bool> IsBanned(string whoId, string byWhomId)
        {
            return (await GetByIdAsync(whoId, byWhomId)) != default;
        }

        public Task<UsersBansDatamodel> GetByIdAsync(string userId, string bannedById)
        {
            return _dbContext.UsersBans.FirstOrDefaultAsync(x => x.BannedByID == bannedById && x.BannedID == userId);
        }
    }
}