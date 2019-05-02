using System.ComponentModel.DataAnnotations;

namespace VibeChat.Web
{
    public class UsersConversationDataModel
    {
        [Key]
        public int UsersConvsID { get; set; }

        public UserInApplication User { get; set; }

        public ConversationDataModel Conversation { get; set; }

    }
}
