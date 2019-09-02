namespace Vibechat.Shared.ApiModels.Conversation
{
    public class CreateChatRequest
    {
        public string ConversationName { get; set; }

        public string DialogUserId { get; set; }

        /// <summary>
        ///     this can be null depending on whether the user has chosen option
        ///     to upload a picture for the group thumbnail
        /// </summary>
        public string ImageUrl { get; set; }

        public bool IsGroup { get; set; }

        public bool IsPublic { get; set; }

        public bool IsSecure { get; set; }

        public string DeviceId { get; set; }
    }
}