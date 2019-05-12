using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VibeChat.Web.Data.DataModels
{
    public class AttachmentKindDataModel
    {
        [Key]
        public string Name { get; set; }
    }
}
