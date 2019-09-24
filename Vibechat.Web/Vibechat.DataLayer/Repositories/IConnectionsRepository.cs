using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface IConnectionsRepository : IAsyncRepository<UserConnectionDataModel>
    {
        ValueTask<UserConnectionDataModel> GetByIdAsync(string connectionId);
    }
}
