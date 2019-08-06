
using Vibechat.Web.Data.Messages;

namespace Vibechat.Web.ApiModels
{
    public class UpdateAuthKeyRequest
    {
        public int chatId { get; set; }

        public string AuthKeyId { get; set; }

        public string deviceId { get; set; }
    }

    public class FindUsersInChatRequest
    {
        public string UsernameToFind { get; set; }

        public int ChatId { get; set; }
    }

    public class SetLastMessageRequest
    {
        public int chatId { get; set; }

        public int messageId { get; set; }
    }

    public class GetAttachmentsRequest
    {
        public AttachmentKind kind { get; set; }

        public int conversationId { get; set; }

        public int offset { get; set; }

        public int count { get; set; }
    }

    public class BanRequest
    {
        public string userId { get; set; }

        public int conversationId { get; set; }
    }

    public class ChangeConversationPublicStateRequest
    {
        public int conversationId { get; set; }
    }
}