using System.ComponentModel.DataAnnotations.Schema;
using VibeChat.Web;
using Vibechat.Web.Data.Conversations;

namespace Vibechat.Web.Data.DataModels
{
    public class ChatRoleDataModel
    {
        public int ChatId { get; set; }

        [ForeignKey("ChatId")] public virtual ConversationDataModel Chat { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")] public virtual AppUser User { get; set; }

        public ChatRole RoleId { get; set; }

        [ForeignKey("RoleId")] public RoleDataModel Role { get; set; }
    }
}