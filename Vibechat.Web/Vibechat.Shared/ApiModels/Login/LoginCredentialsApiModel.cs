namespace Vibechat.Shared.ApiModels.Login
{
    /// <summary>
    ///     Credentials to pass to an API to log in, receive Jwt token back
    /// </summary>
    public class LoginCredentialsApiModel
    {
        /// <summary>
        /// Firebase-issued JWT token.
        /// </summary>
        public string UidToken { get; set; }


        public string PhoneNumber { get; set; }
    }
}