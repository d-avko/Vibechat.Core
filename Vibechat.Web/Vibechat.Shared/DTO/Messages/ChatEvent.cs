namespace Vibechat.Shared.DTO.Messages
{
    public class ChatEvent
    {
        //Why or 'because of who' this event occured
        public string Actor;
        //banned, kicked, joined, invited, left
        public ChatEventType Type;

        public string UserInvolved;

        public string ActorName;

        public string UserInvolvedName;
    }
}