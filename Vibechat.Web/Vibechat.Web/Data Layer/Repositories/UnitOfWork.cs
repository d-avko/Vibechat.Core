using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
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
