using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Background color on picture by default
        /// </summary>
        public string PictureBackground { get; set; }

        /// <summary>
        /// if the image is set, this points to url where the image is.
        /// </summary>  
        public string ImageUrl { get; set; }

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

    }
}
