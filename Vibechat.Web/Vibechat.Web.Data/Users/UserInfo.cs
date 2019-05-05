using System;

namespace VibeChat.Web.ChatData
{
    /// <summary>
    /// Typical user in conversation
    /// </summary>
    public class UserInfo
    {
        public string Id { get; set; }

        /// <summary>
        /// First Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Last name (if set)
        /// </summary>
        public string LastName { get; set; }


        public string UserName { get; set; }

        /// <summary>
        /// Time last seen
        /// </summary>
        public DateTime LastSeen { get; set; }

        /// <summary>
        /// Background of thumbnail(if no picture is set)
        /// </summary>
        public string ProfilePicRgb { get; set; }

        /// <summary>
        /// Thumbnail picture URL
        /// </summary>
        public string ProfilePicImageUrl { get; set; }

        /// <summary>
        /// Connection id needed in signalR
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// Indicates if user is online
        /// </summary>
        public bool IsOnline { get; set; }
    }
}
