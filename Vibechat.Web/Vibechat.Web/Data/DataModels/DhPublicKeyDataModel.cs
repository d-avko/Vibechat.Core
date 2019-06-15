using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data.DataModels
{
    public class DhPublicKeyDataModel
    {
        [Key]
        public int Id { get; set; }

        public string Modulus { get; set; }

        public string Generator { get; set; }
    }
}
