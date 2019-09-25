using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;
using Vibechat.DataLayer.Repositories;
using Vibechat.DataLayer.Repositories.Specifications.Connections;

namespace Vibechat.BusinessLogic.Services.Connections
{
    public class ConnectionsService
    {
        private readonly IConnectionsRepository _connections;
        private readonly ILogger<ConnectionsService> _logger;

        public ConnectionsService(IConnectionsRepository connections, ILogger<ConnectionsService> logger)
        {
            _connections = connections;
            _logger = logger;
        }

        /// <summary>
        /// Returns list of connections of specified user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<IReadOnlyList<UserConnectionDataModel>> GetConnections(string userId)
        {
            return _connections.ListAsync(new GetConnectionsOfSpec(userId));
        }

        /// <summary>
        /// Removes connection from user's connections list.
        /// Doesn't throw if fails.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> RemoveConnection(string connectionId, string userId)
        {
            var entry = await _connections.GetByIdAsync(connectionId);

            if(entry == null)
            {
                _logger.LogWarning($"Failed to remove connection for {userId} as it was not present.");
                return false;
            }

            await _connections.DeleteAsync(entry);

            return true;
        }
         
        /// <summary>
        /// Adds connection for specified user.
        /// Doesn't throw if fails.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="userId"></param>
        /// <returns>Is successful?</returns>
        public async Task<UserConnectionDataModel> AddConnection(string connectionId, string userId)
        {
            var entry = await _connections.GetByIdAsync(connectionId);

            if(entry != null)
            {
                _logger.LogWarning($"Failed to add connection for {userId} as it exists.");
                return null;
            }

            return await _connections.AddAsync(UserConnectionDataModel.Create(connectionId, userId));
        }
    }
}
