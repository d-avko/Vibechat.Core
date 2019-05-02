using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VibeChat.Web.UserProviders
{
    public interface ICustomHubUserIdProvider
    {
        string GetUserId(HubCallerContext context);
    }
}
