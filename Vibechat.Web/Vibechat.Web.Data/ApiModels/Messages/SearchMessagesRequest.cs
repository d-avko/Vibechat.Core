namespace Vibechat.Web.ApiModels.Messages
{
    public class SearchMessagesRequest
    {
        public string deviceId { get; set; }
        
        public string searchString { get; set; }
        
        public int offset { get; set; }
        
        public int count { get; set; }
    }
}