using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibechat.Web.Services.ChatDataProviders
{
    public interface IChatDataProvider
    {
        string GetProfilePictureUrl();

        string GetGroupPictureUrl();
    }
}
