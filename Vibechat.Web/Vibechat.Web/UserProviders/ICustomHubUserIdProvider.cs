using Microsoft.AspNetCore.SignalR;

namespace VibeChat.Web.UserProviders
{
    public interface ICustomHubUserIdProvider
    {
        string GetUserId(HubCallerContext context);
    }
}