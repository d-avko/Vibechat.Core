using System.ComponentModel.DataAnnotations;

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


        /// <summary>
        /// if the image is set, this points to url where the image is.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Indicates if this is a group
        /// </summary>
        public bool IsGroup { get; set; }

        public UserInApplication Creator { get; set; }
    }
}
