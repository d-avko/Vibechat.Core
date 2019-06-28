using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data.DataModels
{
    public class ContactsDataModel
    {
        public string FirstUserID { get; set; }
         
        public string SecondUserID { get; set; }

        [ForeignKey("FirstUserID")]
        public virtual UserInApplication User { get; set; }

        [ForeignKey("SecondUserID")]
        public virtual UserInApplication Contact { get; set; }
    }
}
