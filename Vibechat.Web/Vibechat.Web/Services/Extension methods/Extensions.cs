using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.ChatData.Messages;
using VibeChat.Web;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Extensions
{
    public static class Extensions
    {
        public static UserInfo ToUserInfo(this UserInApplication user)
        {
            return new UserInfo()
            {
                Id = user.Id,
                Name = user.FirstName,
                ImageUrl = user.ProfilePicImageURL,
                LastName = user.LastName,
                LastSeen = user.LastSeen,
                UserName = user.UserName,
                ConnectionId = user.ConnectionId,
                IsOnline = user.IsOnline,
                IsPublic = user.IsPublic
            };
        }

        public static MessageAttachment ToMessageAttachment(this MessageDataModel value)
        {
            return new MessageAttachment()
            {
                AttachmentKind = value.AttachmentInfo.AttachmentKind.Name,
                ContentUrl = value.AttachmentInfo.ContentUrl,
                AttachmentName = value.AttachmentInfo.AttachmentName,
                ImageHeight = value.AttachmentInfo.ImageHeight,
                ImageWidth = value.AttachmentInfo.ImageWidth
            };
        }

        public static ConversationTemplate ToConversationTemplate(
            this ConversationDataModel value, 
            List<UserInfo> participants,
            List<Message> messages,
            UserInApplication dialogUser)
        {
            return new ConversationTemplate()
            {
                Name = value.Name,
                ConversationID = value.ConvID,
                DialogueUser = dialogUser?.ToUserInfo(),
                IsGroup = value.IsGroup,
                ThumbnailUrl = value.ThumbnailUrl,
                FullImageUrl = value.FullImageUrl,
                Participants = participants,
                Messages = messages,
                Creator = value.Creator.ToUserInfo()
            };
        }

        public static Message ToMessage(this MessageDataModel value)
        {
            return new Message()
            {
                Id = value.MessageID,
                ConversationID = value.ConversationID,
                MessageContent = value.MessageContent,
                TimeReceived = value.TimeReceived,
                User = value.User?.ToUserInfo(),
                AttachmentInfo = value?.ToMessageAttachment(),
                IsAttachment = value.IsAttachment
            };
        }
    }
}
