using System.ComponentModel.DataAnnotations;
using Vibechat.Web.Data.DataModels;

namespace VibeChat.Web
{ 
    public class ConversationDataModel
    {
        [Key]
        public int ConvID { get; set; }

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

        public DhPublicKeyDataModel PublicKey { get; set; }

        public UserInApplication Creator { get; set; }
    }
}
