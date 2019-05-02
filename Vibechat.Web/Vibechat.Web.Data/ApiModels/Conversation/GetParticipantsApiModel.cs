using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeChat.Web.ApiModels
{
    /// <summary>
    /// Model to get participants of conversation
    /// </summary>
    public class GetParticipantsApiModel
    {
        public int ConvId { get; set; }
    }
}
