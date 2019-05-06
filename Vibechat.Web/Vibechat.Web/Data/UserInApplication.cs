using Microsoft.AspNetCore.Identity;
using System;

namespace VibeChat.Web
{
    public class UserInApplication : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime LastSeen { get; set; }

        /// <summary>
        /// Url that points to a place the image is.
        /// </summary>
        public string ProfilePicImageURL { get; set; }

        /// <summary>
        /// Connection id needed for signalR
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// Indicates if user is online
        /// </summary>
        public bool IsOnline { get; set; }
    }
}