using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.Messages;

namespace VibeChat.Web.Data.DataModels
{
    public class AttachmentKindDataModel
    {
        [Key]
        public AttachmentKind Kind { get; set; }
    }
}
