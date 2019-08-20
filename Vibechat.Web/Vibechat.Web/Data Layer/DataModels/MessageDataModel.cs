using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using VibeChat.Web.Data.DataModels;
using Vibechat.Web.Data_Layer.DataModels;
using Vibechat.Web.DTO.Messages;

namespace VibeChat.Web
{
    public enum MessageState
    {
        Delivered = 1,
        Read = 2
    }

    public class MessageDataModel
    {
        [Key] public int MessageID { get; set; }

        public MessageState State { get; set; }

        public AppUser User { get; set; }

        //content of a message: text

        public string MessageContent { get; set; }

        public MessageAttachmentDataModel AttachmentInfo { get; set; }

        public MessageType Type { get; set; }
        
        public ChatEventDataModel Event { get; set; }
        
        public MessageDataModel ForwardedMessage { get; set; }

        //Id of a conversation where this message was sent 

        public int ConversationID { get; set; }

        [ForeignKey("ConversationID")] 
        public virtual ConversationDataModel Chat { get; set; }

        //time when this message was received
        public DateTime TimeReceived { get; set; }

        /// <summary>
        ///     if this message is from private chat, store payload there.
        /// </summary>
        public string EncryptedPayload { get; set; }

        public static MessageDataModel Create(AppUser user, int chatId)
        {
            return new MessageDataModel()
            {
                User = user,
                ConversationID = chatId,
                TimeReceived = DateTime.UtcNow,
                State = MessageState.Delivered
            };
        }
    }
}