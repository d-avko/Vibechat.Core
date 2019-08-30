using Microsoft.AspNetCore.SignalR;

namespace Vibechat.BusinessLogic.UserProviders
{
    public interface ICustomHubUserIdProvider
    {
        string GetUserId(HubCallerContext context);
    }
}