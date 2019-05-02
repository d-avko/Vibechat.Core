namespace VibeChat.Web.ApiModels
{
    public class AddToConversationApiModel
    {
        //User to add
        public string UserId { get; set; }
        //id of a conversation in which to add
        public int ConvId { get; set; }
    }
}
