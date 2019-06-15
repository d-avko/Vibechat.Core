using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vibechat.Web.Data.Conversations;
using VibeChat.Web.Controllers;

namespace VibeChat.Web.ChatData
{
    public class ConversationTemplate
    {
        public int ConversationID { get; set; }

        /// <summary>
        /// If this conversation is Dialogue, then this field points to recepient
        /// </summary>
        public UserInfo DialogueUser { get; set; }

        /// <summary>
        /// Conversation name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// if the image is set, this points to url where the image is.
        /// </summary>  
        public string ThumbnailUrl { get; set; }

        public string FullImageUrl { get; set; }

        public UserInfo Creator { get; set; }

        /// <summary>
        /// Indicates if this is a group
        /// </summary>
        public bool IsGroup { get; set; }

        /// <summary>
        /// Conversation Messages
        /// </summary>
        public List<Message> Messages { get; set; }

        /// <summary>
        /// List of conversation participants
        /// </summary>
        public List<UserInfo> Participants { get; set; }

        public bool IsSecure { get; set; }

        /// <summary>
        /// If this is secret chat, this property is a hash of first 1024 bits of auth key.
        /// </summary>
        public string AuthKeyId { get; set; }

        public DhPublicKey PublicKey { get; set; }

        /// <summary>
        /// Field indicating whether calling user can message in this conversation.
        /// </summary>
        public bool IsMessagingRestricted { get; set; }
        
        /// <summary>
        /// This field is filled only once in <see cref="ConversationsController.GetAll()"/> method
        /// </summary>
        public int MessagesUnread { get; set; }
    }
}
