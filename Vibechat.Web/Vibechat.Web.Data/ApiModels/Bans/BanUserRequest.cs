using System;
using System.Collections.Generic;
using System.Text;

namespace Vibechat.Web.Data.ApiModels.Bans
{
    public class BanUserRequest
    {
        public string UserId { get; set; }

        /// <summary>
        /// Can be null
        /// </summary>
        public int? ConversationId { get; set; }
    }
}
