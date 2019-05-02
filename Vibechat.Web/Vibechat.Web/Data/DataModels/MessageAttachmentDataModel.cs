using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VibeChat.Web.Data.DataModels
{
    public class MessageAttachmentDataModel
    {
        [Key]
        public int AttachmentID { get; set; }
        public string ContentUrl { get; set; }

        public AttachmentKindDataModel AttachmentKind { get; set; }
    }
}
