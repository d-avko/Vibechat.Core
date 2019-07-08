using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.ChatData.Messages;
using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Data.DataModels;
using Vibechat.Web.Services.Extension_methods;
using VibeChat.Web;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Extensions
{
    public static class Extensions
    {
        public static UserInfo ToUserInfo(this AppUser user)
        {
            return new UserInfo()
            {
                Id = user.Id,
                Name = user.FirstName,
                ImageUrl = user.ProfilePicImageURL,
                FullImageUrl = user.FullImageUrl,
                LastName = user.LastName,
                LastSeen = user.LastSeen.ToUTCString(),
                UserName = user.UserName,
                ConnectionId = user.ConnectionId,
                IsOnline = user.IsOnline,
                IsPublic = user.IsPublic
            };
        }

        public static MessageAttachment ToMessageAttachment(this MessageAttachmentDataModel value)
        {
            return new MessageAttachment()
            {
                AttachmentKind = value.AttachmentKind.Name,
                ContentUrl = value.ContentUrl,
                AttachmentName = value.AttachmentName,
                ImageHeight = value.ImageHeight,
                ImageWidth = value.ImageWidth,
                FileSize = value.FileSize
            };
        }

        public static DhPublicKey ToDhPublicKey(this DhPublicKeyDataModel value)
        {
            return new DhPublicKey()
            {
                Generator = value.Generator,
                Modulus = value.Modulus
            };
        }

        public static ConversationTemplate ToConversationTemplate(
            this ConversationDataModel value, 
            List<UserInfo> participants,
            List<Message> messages,
            AppUser dialogUser,
            DhPublicKeyDataModel key,
            string deviceId)
        {
            return new ConversationTemplate()
            {
                Name = value.Name,
                ConversationID = value.Id,
                DialogueUser = dialogUser?.ToUserInfo(),
                IsGroup = value.IsGroup,
                ThumbnailUrl = value.ThumbnailUrl,
                FullImageUrl = value.FullImageUrl,
                Participants = participants,
                Messages = messages,
                Creator = value.Creator.ToUserInfo(),
                AuthKeyId = value.AuthKeyId,
                IsSecure = value.IsSecure,
                PublicKey = key?.ToDhPublicKey(),
                DeviceId = deviceId
            };
        }

        public static Message ToMessage(this MessageDataModel value)
        {
            return new Message()
            {
                Id = value.MessageID,
                ConversationID = value.ConversationID,
                MessageContent = value.MessageContent,
                TimeReceived = value.TimeReceived.ToUTCString(),
                User = value.User?.ToUserInfo(),
                AttachmentInfo = value.AttachmentInfo?.ToMessageAttachment(),
                IsAttachment = value.IsAttachment,
                ForwardedMessage = value.ForwardedMessage?.ToMessage(),
                State = value.State,
                EncryptedPayload = value.EncryptedPayload
            };
        }
    }
}
