using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeChat.Web.ApiModels
{
    /// <summary>
    /// Api request for receiving messages
    /// </summary>
    public class GetMessagesApiModel
    {
        public int ConversationID { get; set; }

        public int MesssagesOffset { get; set; }

        public int Count { get; set; }
    }
}
