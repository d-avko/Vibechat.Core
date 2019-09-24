using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public class ConnectionsRepository : BaseRepository<UserConnectionDataModel>, IConnectionsRepository
    {
        public ConnectionsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public ValueTask<UserConnectionDataModel> GetByIdAsync(string connectionId)
        {
            return _dbContext.UserConnections.FindAsync(connectionId);
        }
    }
}
