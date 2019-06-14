using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibechat.Web.Data.DataModels
{
    public class SessionDataModel
    {
        /// <summary>
        /// Length is 2048 bits == 256 bytes.
        /// </summary>
        public string AuthKey { get; set; }
    }
}
