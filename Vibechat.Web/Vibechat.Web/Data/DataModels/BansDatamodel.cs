using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data.DataModels
{
    public class BansDatamodel
    {
        [Key]
        public int Ban_Id { get; set; }
        public UserInApplication WhoBanned { get; set; }

        public UserInApplication Banned { get; set; }
    }
}
