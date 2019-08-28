using Vibechat.Web.ChatData.Messages;
using Vibechat.Web.DTO.Messages;

namespace VibeChat.Web.ChatData
{
    public class Message
    {
        public int Id { get; set; }
        public AppUserDto User { get; set; }
        public string MessageContent { get; set; }
        public int ConversationID { get; set; }
        public string TimeReceived { get; set; }

        public Message ForwardedMessage { get; set; }
        
        public MessageType Type { get; set; }
        
        public ChatEvent Event { get; set; }

        public MessageAttachment AttachmentInfo { get; set; }

        public MessageState State { get; set; }

        /// <summary>
        ///     if this message is from private chat, store payload there.
        /// </summary>
        public string EncryptedPayload { get; set; }
    }
}