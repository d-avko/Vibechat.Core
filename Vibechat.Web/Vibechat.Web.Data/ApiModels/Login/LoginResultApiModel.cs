using VibeChat.Web.ChatData;

namespace Vibechat.Web.ApiModels
{
    /// <summary>
    ///     Class used to pass info to a user in a result of a request
    ///     to log in
    /// </summary>
    public class LoginResultApiModel
    {
        /// <summary>
        ///     As a result of a successfull login we'll get bearer token.
        /// </summary>
        public string Token { get; set; }

        public string RefreshToken { get; set; }

        public AppUserDto Info { get; set; }

        /// <summary>
        ///     Flag indicating if this user
        ///     was auto-registered and his username needs to be changed.
        /// </summary>
        public bool IsNewUser { get; set; }
    }
}