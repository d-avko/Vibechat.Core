using System;
using System.Collections.Generic;
using System.Text;

namespace Vibechat.Web.Data.ApiModels.Messages
{
    public class DeleteMessagesRequest
    {
        public List<int> MessagesId { get; set; }

        public int ConversationId { get; set; }
    }
}
