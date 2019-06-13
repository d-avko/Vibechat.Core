using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data.DataModels
{
    public class UserSessionDataModel
    {
        public UserInApplication User { get; set; }

        public SessionDataModel Session { get; set; }
    }
}
