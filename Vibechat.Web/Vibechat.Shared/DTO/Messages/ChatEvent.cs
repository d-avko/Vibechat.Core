using Newtonsoft.Json;

namespace Vibechat.Shared.DTO.Messages
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ChatEvent
    {
        public string Actor { get; set; }
        
        public ChatEventType Type { get; set; }

        public string UserInvolved { get; set; }

        public string ActorName { get; set; }

        public string UserInvolvedName { get; set; }
    }
}