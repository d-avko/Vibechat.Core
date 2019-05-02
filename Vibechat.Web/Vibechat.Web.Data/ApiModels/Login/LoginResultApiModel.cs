using System;

namespace Vibechat.Web.ApiModels
{
    /// <summary>
    /// Class used to pass info to a user in a result of a request
    /// to log in
    /// </summary>
    public class LoginResultApiModel
    {
        /// <summary>
        /// As a result of a successfull login we'll get bearer token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Additional info about the user
        /// </summary>
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string UserName { get; set; }

        public string ID { get; set; }

        public string ImageUrl { get; set; }

        public string Email { get; set; }

        public string ProfilePicRgb { get; set; }

        public string ProfilePicImageURL { get; set; }

        public DateTime LastSeen { get; set; }

    }
}
