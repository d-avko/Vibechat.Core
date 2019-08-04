using System.ComponentModel.DataAnnotations.Schema;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.DataModels
{
    public class LastMessageDataModel
    {
        public int MessageID { get; set; }

        public int ChatID { get; set; }

        public string UserID { get; set; }

        [ForeignKey("UserID")] public virtual AppUser User { get; set; }

        [ForeignKey("ChatID")] public virtual ConversationDataModel Conversation { get; set; }

        [ForeignKey("MessageID")] public virtual MessageDataModel Message { get; set; }
    }
}