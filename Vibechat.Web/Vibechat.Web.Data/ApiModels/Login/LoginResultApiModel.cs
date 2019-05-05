using System;
using VibeChat.Web.ChatData;

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

        public UserInfo Info { get; set; }

    }
}
