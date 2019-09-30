using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Vibechat.DataLayer.DataModels
{
    public class ConversationDataModel
    {
        [Key] public int Id { get; set; }

        /// <summary>
        ///     Conversation name
        /// </summary>
        public string Name { get; set; }


        public string ThumbnailUrl { get; set; }

        public string FullImageUrl { get; set; }

        /// <summary>
        ///     Indicates if this is a group
        /// </summary>
        public bool IsGroup { get; set; }

        public bool IsPublic { get; set; }

        public bool IsSecure { get; set; }

        public string AuthKeyId { get; set; }

        public int? PublicKeyId { get; set; }

        [ForeignKey("PublicKeyId")] public virtual DhPublicKeyDataModel PublicKey { get; set; }

        public ICollection<ConversationsBansDataModel> BannedUsers { get; set; }

        public ICollection<ChatRoleDataModel> Roles { get; set; }

        //Field used by ef
        public ICollection<UsersConversationDataModel> Participants { get; set; }

        public ICollection<LastMessageDataModel> LastMessages { get; set; }
        
        [NotMapped]
        public string DeviceId { get; set; }

        [NotMapped]
        public MessageDataModel LastMessage { get; set; }
        
        [NotMapped]
        public int ClientLastMessage { get; set; }
        
        [NotMapped]
        public int UnreadCount { get; set; }

        [NotMapped]
        public ChatRoleDataModel Role { get; set; }

        [NotMapped]
        public bool IsMessagingRestricted { get; set; }

        //Field used by dto
        [NotMapped]
        public IEnumerable<AppUser> participants { get; set; }
         
        public AppUser GetDialogUser(string userId)
        {
            return Participants?.FirstOrDefault(x => x.UserID != userId)?.User;
        }
    }
}