using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime LastSeen { get; set; }

        /// <summary>
        ///     Url that points to a place the image is.
        /// </summary>
        public string ProfilePicImageURL { get; set; }

        public string FullImageUrl { get; set; }

        /// <summary>
        /// Connections of this user.
        /// </summary>
        public ICollection<UserConnectionDataModel> Connections { get; set; }
         
        /// <summary>
        ///     Indicates if user is online
        /// </summary>
        public bool IsOnline { get; set; }

        public bool IsPublic { get; set; }

        public string RefreshToken { get; set; }

        public bool IsAdmin { get; set; }
    }
}