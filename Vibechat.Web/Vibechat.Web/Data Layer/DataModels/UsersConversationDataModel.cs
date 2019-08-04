using System.ComponentModel.DataAnnotations.Schema;

namespace VibeChat.Web
{
    public class UsersConversationDataModel
    {
        public string UserID { get; set; }

        public int ChatID { get; set; }

        [ForeignKey("UserID")] public virtual AppUser User { get; set; }

        [ForeignKey("ChatID")] public virtual ConversationDataModel Conversation { get; set; }

        /// <summary>
        ///     Secure chat deviceId
        /// </summary>
        public string DeviceId { get; set; }
    }
}