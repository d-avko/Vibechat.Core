using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vibechat.Web.Data.DataModels;

namespace VibeChat.Web
{ 
    public class ConversationDataModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Conversation name
        /// </summary>
        public string Name { get; set; }


        public string ThumbnailUrl { get; set; }

        public string FullImageUrl { get; set; }

        /// <summary>
        /// Indicates if this is a group
        /// </summary>
        public bool IsGroup { get; set; }

        public bool IsPublic { get; set; }

        public bool IsSecure { get; set; }

        public string AuthKeyId { get; set; }

        public int? PublicKeyId { get; set; }

        [ForeignKey("PublicKeyId")]
        public virtual DhPublicKeyDataModel PublicKey { get; set; }
    }
}
