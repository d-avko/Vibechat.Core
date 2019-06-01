using System;
using System.ComponentModel.DataAnnotations;
using VibeChat.Web.Data.DataModels;

namespace VibeChat.Web
{
    /// <summary>
    /// Data model for messages
    /// </summary>
    public class MessageDataModel
    {
        [Key]
        public int MessageID { get; set; }

        //Who sent this message

        public UserInApplication User { get; set; }

        //content of a message: text or image

        public string MessageContent { get; set; }

        public MessageAttachmentDataModel AttachmentInfo { get; set; }

        public bool IsAttachment { get; set; }

        //Id of a conversation where this message was sent 

        public int ConversationID { get; set; }

        //time when this message was received
        public DateTime TimeReceived { get; set; }
    }
}
