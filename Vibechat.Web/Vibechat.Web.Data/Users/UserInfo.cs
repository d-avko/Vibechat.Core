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
        /// Thumbnail picture URL
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Connection id needed in signalR
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// Indicates if user is online
        /// </summary>
        public bool IsOnline { get; set; }

        public bool IsPublic { get; set; }

        /// <summary>
        /// Field indicates if user who is querying the data was banned by this user.
        /// This field is filled automatically.
        /// </summary>
        public bool IsMessagingRestricted { get; set; }

        public bool IsBlocked { get; set; }
    }
}
