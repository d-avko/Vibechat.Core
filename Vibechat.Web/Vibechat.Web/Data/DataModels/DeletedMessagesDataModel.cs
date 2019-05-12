using System.ComponentModel.DataAnnotations;

namespace VibeChat.Web
{
    /// <summary>
    /// Data model for deleted messages
    /// </summary>
    public class DeletedMessagesDataModel
    {
        [Key]
        //deleted message
        public int Id { get; set; }

        public MessageDataModel Message { get; set; }
      
        //user that deleted that message
        public string UserId { get; set; } 
    }
}
