using System.Threading.Tasks;

namespace Vibechat.DataLayer.Repositories
{
    public class UnitOfWork
    {
        public UnitOfWork(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public ApplicationDbContext DbContext { get; }

        public Task Commit()
        {
            return DbContext.SaveChangesAsync();
        }
    }
}