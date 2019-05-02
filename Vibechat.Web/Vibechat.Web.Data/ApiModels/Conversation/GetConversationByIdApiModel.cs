using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeChat.Web.ApiModels
{
    public class GetConversationByIdApiModel
    {
        public int ConversationId { get; set; } 

        /// <summary>
        /// Who requested the info
        /// </summary>
        public string UserId { get; set; }
    }
}
