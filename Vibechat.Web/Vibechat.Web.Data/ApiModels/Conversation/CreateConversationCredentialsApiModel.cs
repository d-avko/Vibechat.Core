namespace VibeChat.Web.ApiModels
{
    public class CreateConversationCredentialsApiModel
    {
        public string ConversationName { get; set; }

        public string CreatorId { get; set; }

        public string DialogUserId { get; set; }
        /// <summary>
        /// this can be null depending on whether the user has chosen option 
        /// to upload a picture for the group thumbnail
        /// </summary>
        public string ImageUrl { get; set; }

        public bool IsGroup { get; set; }

        public bool IsPublic { get; set; }
    }
}
