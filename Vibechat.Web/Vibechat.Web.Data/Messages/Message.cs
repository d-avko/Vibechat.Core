using System;
using System.Collections.Generic;
using Vibechat.Web.ChatData.Messages;

namespace VibeChat.Web.ChatData
{
    public class Message
    {
        public UserInfo User { get; set; }
        public string MessageContent { get; set; }
        public int ConversationID { get; set; }
        public DateTime TimeReceived{ get; set; }

        public IEnumerable<MessageAttachment> Attachments { get; set; }
    }
}
