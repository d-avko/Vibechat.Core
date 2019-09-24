using System.ComponentModel.DataAnnotations.Schema;

namespace Vibechat.DataLayer.DataModels
{
    public class UsersConversationDataModel
    {
        public string UserID { get; set; }

        public int ChatID { get; set; }

        [ForeignKey("UserID")] public virtual AppUser User { get; set; }

        [ForeignKey("ChatID")] public virtual ConversationDataModel Conversation { get; set; }

        /// <summary>
        ///     Secure chat deviceId
        /// </summary>
        public string DeviceId { get; set; }

        public static UsersConversationDataModel Create(string userId, ConversationDataModel chat, string deviceId = null)
        {
            return new UsersConversationDataModel
            {
                Conversation = chat,
                UserID = userId,
                DeviceId = deviceId
            };
        }

        public static UsersConversationDataModel Create(string userId, int chatId, string deviceId = null)
        {
            return new UsersConversationDataModel
            {
                ChatID = chatId,
                UserID = userId,
                DeviceId = deviceId
            };
        }
    }
}