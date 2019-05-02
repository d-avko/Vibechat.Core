using System.Collections.Generic;
using VibeChat.Web.ChatData;

namespace VibeChat.Web.ApiModels
{
    public class GetParticipantsResultApiModel
    {
        public List<UserInfo> Participants { get; set; }
    }
}
