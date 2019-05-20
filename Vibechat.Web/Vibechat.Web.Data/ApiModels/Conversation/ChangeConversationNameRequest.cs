using System;
using System.Collections.Generic;
using System.Text;

namespace Vibechat.Web.Data.ApiModels.Conversation
{
    public class ChangeConversationNameRequest
    {
        public string Name { get; set; }

        public int ConversationId { get; set; }
    }
}
