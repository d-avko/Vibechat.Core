using System;

namespace VibeChat.Web.ChatData
{
    public class Message
    {
        public UserInfo User { get; set; }
        public string MessageContent { get; set; }
        public int ConversationID { get; set; }
        public DateTime TimeReceived{ get; set; }
    }
}
