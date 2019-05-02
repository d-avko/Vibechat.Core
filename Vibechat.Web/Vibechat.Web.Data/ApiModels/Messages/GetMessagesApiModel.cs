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
        public int ConvID { get; set; }
    }
}
