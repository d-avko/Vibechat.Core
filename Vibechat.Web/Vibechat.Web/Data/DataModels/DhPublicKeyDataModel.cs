using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;

namespace Vibechat.Web.Data.DataModels
{
    public class DhPublicKeyDataModel
    {
        /// <summary>
        /// Represents a chat where it's used.
        /// </summary>
        [Key]
        public int Id { get; set; }

        public string Modulus { get; set; }

        public string Generator { get; set; }

        [ForeignKey("Id")]
        public virtual ConversationDataModel Chat { get; set; }
    }
}
