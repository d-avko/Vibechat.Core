using System.Collections.Generic;
using VibeChat.Web.ChatData;

namespace VibeChat.Web.ApiModels
{
    /// <summary>
    ///     Server's response to request of messages
    /// </summary>
    public class GetMessagesResultApiModel
    {
        public List<Message> Messages { get; set; }
    }
}