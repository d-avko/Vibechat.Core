using System.Collections.Generic;

namespace Vibechat.Shared.ApiModels.Messages
{
    public class DeleteMessagesRequest
    {
        public List<int> MessagesId { get; set; }

        public int ConversationId { get; set; }
    }
}