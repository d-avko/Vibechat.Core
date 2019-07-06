﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Vibechat.Web.ChatData.Messages
{
    public class MessageAttachment
    {
        /// <summary>
        /// Relative url
        /// </summary>
        public string ContentUrl { get; set; }

        public string AttachmentName { get; set; }

        public string AttachmentKind { get; set; }

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public long FileSize { get; set; }
    }
}
