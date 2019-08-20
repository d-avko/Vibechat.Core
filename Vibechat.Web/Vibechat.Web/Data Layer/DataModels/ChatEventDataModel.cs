using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.DataModels
{
    public class ChatEventDataModel
    {
        [Key]
        public int Id { get; set; }
        public string ActorId { get; set; }

        [ForeignKey("ActorId")] public virtual AppUser Actor { get; set; }
        
        public string UserInvolvedId { get; set; }
        
        [ForeignKey("UserInvolvedId")] public virtual AppUser UserInvolved { get; set; }
        
        public ChatEventType EventType { get; set; }

        public static ChatEventDataModel Create(string actor, string userInvolvedId, ChatEventType eventType)
        {
            return new ChatEventDataModel()
            {
                ActorId = actor,
                UserInvolvedId = userInvolvedId,
                EventType = eventType
            };
        }
    }
}