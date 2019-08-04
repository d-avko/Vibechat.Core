using System.ComponentModel.DataAnnotations.Schema;
using VibeChat.Web;

namespace Vibechat.Web.Data.DataModels
{
    public class ConversationsBansDataModel
    {
        public string UserID { get; set; }

        public int ChatID { get; set; }

        [ForeignKey("UserID")] public virtual AppUser BannedUser { get; set; }

        [ForeignKey("ChatID")] public virtual ConversationDataModel Conversation { get; set; }
    }
}