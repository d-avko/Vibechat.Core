using Vibechat.Web.Data.Conversations;

namespace VibeChat.Web.ChatData
{
    /// <summary>
    ///     Typical user in conversation
    /// </summary>
    public class AppUserDto
    {
        public string Id { get; set; }

        /// <summary>
        ///     First Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Last name (if set)
        /// </summary>
        public string LastName { get; set; }


        public string UserName { get; set; }

        /// <summary>
        ///     Time last seen
        /// </summary>
        public string LastSeen { get; set; }

        /// <summary>
        ///     Thumbnail picture URL
        /// </summary>
        public string ImageUrl { get; set; }

        public string FullImageUrl { get; set; }

        /// <summary>
        ///     Connection id needed in signalR
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        ///     Indicates if user is online
        /// </summary>
        public bool IsOnline { get; set; }

        public bool IsPublic { get; set; }

        /// <summary>
        ///     Field indicates if user who is querying the data was banned by this user.
        ///     This field is filled automatically.
        /// </summary>
        public bool IsMessagingRestricted { get; set; }

        /// <summary>
        ///     Field indicates if user who is querying the data banned this user.
        ///     This field is filled automatically.
        /// </summary>
        public bool IsBlocked { get; set; }

        /// <summary>
        ///     Field filled by methods in <see cref="ConversationsController" /> ,
        ///     indicating whether user was banned in specific conversation.
        /// </summary>
        public bool IsBlockedInConversation { get; set; }

        /// <summary>
        ///     Role of user in specific chat.
        /// </summary>
        public ChatRoleDto ChatRole { get; set; }
    }
}