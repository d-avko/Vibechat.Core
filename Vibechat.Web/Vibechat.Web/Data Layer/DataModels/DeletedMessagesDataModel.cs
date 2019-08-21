using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VibeChat.Web
{
    /// <summary>
    ///     Data model for deleted messages
    /// </summary>
    public class DeletedMessagesDataModel
    {
        [Key]
        //deleted message
        public int MessageID { get; set; }

        [ForeignKey("MessageID")] public virtual MessageDataModel Message { get; set; }

        //user that deleted that message
        public string UserId { get; set; }

        public static DeletedMessagesDataModel Create(string userId, MessageDataModel message)
        {
            return new DeletedMessagesDataModel
            {
                UserId = userId,
                Message = message
            };
        }
    }
}