using Microsoft.AspNetCore.SignalR;
using VibeChat.Web.UserProviders;

namespace VibeChat.Web
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