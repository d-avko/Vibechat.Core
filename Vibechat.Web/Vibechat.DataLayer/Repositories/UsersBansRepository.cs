using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
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