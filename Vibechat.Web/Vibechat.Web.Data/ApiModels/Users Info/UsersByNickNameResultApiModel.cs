using System.Collections.Generic;
using VibeChat.Web.ChatData;

namespace Vibechat.Web.ApiModels
{
    public class UsersByNickNameResultApiModel
    {
        public List<FoundUser> UsersFound { get; set; }
    }
}
