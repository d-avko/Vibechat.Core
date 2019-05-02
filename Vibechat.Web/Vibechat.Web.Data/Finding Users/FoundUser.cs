namespace VibeChat.Web.ChatData
{
    /// <summary>
    /// Data class used by server to send data to client (FoundUsersViewModel)
    /// </summary>
    public class FoundUser
    {
        public string ID { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProfilePicRgb { get; set; }
    }
}
