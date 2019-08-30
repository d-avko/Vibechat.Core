using Microsoft.AspNetCore.SignalR;

namespace Vibechat.BusinessLogic.UserProviders
{
    /// <summary>
    ///     Custom user provider for SignalR hubs, using 'UserId' here as claim.
    /// </summary>
    public class DefaultUserIdProvider : ICustomHubUserIdProvider
    {
        public virtual string GetUserId(HubCallerContext connection)
        {
            return connection.User?.FindFirst("UserId")?.Value;
        }
    }
}