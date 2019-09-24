using System.ComponentModel.DataAnnotations.Schema;
using Vibechat.Shared.DTO.Conversations;

namespace Vibechat.DataLayer.DataModels
{
   
    public class ChatRoleDataModel
    {
        public int ChatId { get; set; }

        [ForeignKey("ChatId")] public virtual ConversationDataModel Chat { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")] public virtual AppUser User { get; set; }

        public ChatRole RoleId { get; set; }

        [ForeignKey("RoleId")] public RoleDataModel Role { get; set; }

        public static ChatRoleDataModel Create(int chatId, string userId, ChatRole role)
        {
            return new ChatRoleDataModel
            {
                ChatId = chatId,
                UserId = userId,
                RoleId = role
            };
        }

        public static ChatRoleDataModel Create(ConversationDataModel chat, string userId, ChatRole role)
        {
            return new ChatRoleDataModel
            {
                Chat = chat,
                UserId = userId,
                RoleId = role
            };
        }
    }
}