using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.Conversations;
using VibeChat.Web;
using VibeChat.Web.ChatData;

namespace Vibechat.Web.Data.DataModels
{
    public class ChatRoleDataModel
    {
        public int ChatId { get; set; }

        [ForeignKey("ChatId")]
        public virtual ConversationDataModel Chat { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }

        public ChatRole RoleId { get; set; }

        [ForeignKey("RoleId")]
        public RoleDataModel Role { get; set; }
    }
}
