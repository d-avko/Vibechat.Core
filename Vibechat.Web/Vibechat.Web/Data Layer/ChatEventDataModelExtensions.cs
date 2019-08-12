using Vibechat.Web.Data_Layer.DataModels;

namespace VibeChat.Web
{
    public static class ChatEventDataModelExtensions
    {
        public static ChatEventDataModel Create(this ChatEventDataModel value,
            string actor, string userInvolvedId, ChatEventType eventType)
        {
            value.ActorId = actor;
            value.UserInvolvedId = userInvolvedId;
            value.EventType = eventType;
            return value;
        }
    }
}