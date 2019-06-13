using System;
using System.Collections.Generic;
using System.Text;

namespace Vibechat.Web.Data.ApiModels.misc
{
    public class EncryptedRequest
    {
        public string AuthKeyId { get; set; }

        public string MessageKey { get; set; }

        /// <summary>
        /// Encrypted data encoded in base64 format
        /// </summary>
        public string Data { get; set; }
    }
}
