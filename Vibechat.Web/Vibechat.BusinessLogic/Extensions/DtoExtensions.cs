using System.Collections.Generic;
using System.Linq;
using Vibechat.DataLayer;
using Vibechat.DataLayer.DataModels;
using Vibechat.Shared.DTO.Conversations;
using Vibechat.Shared.DTO.Messages;
using Vibechat.Shared.DTO.Users;

namespace Vibechat.BusinessLogic.Extensions
{
    public static class DtoExtensions
    {
        public static AppUserDto ToAppUserDto(this AppUser user)
        {
            return new AppUserDto
            {
                Id = user.Id,
                Name = user.FirstName,
                ImageUrl = user.ProfilePicImageURL,
                FullImageUrl = user.FullImageUrl,
                LastName = user.LastName,
                LastSeen = user.LastSeen.ToUTCString(),
                UserName = user.UserName,
                IsOnline = user.IsOnline,
                IsPublic = user.IsPublic,
                IsMessagingRestricted = user.IsBlockedInChat,
                ChatRole = user.ChatRole?.ToChatRole()
            };
        }

        public static MessageAttachment ToMessageAttachment(this MessageAttachmentDataModel value)
        {
            return new MessageAttachment
            {
                AttachmentKind = value.AttachmentKind.Kind,
                ContentUrl = value.ContentUrl,
                AttachmentName = value.AttachmentName,
                ImageHeight = value.ImageHeight,
                ImageWidth = value.ImageWidth,
                FileSize = value.FileSize
            };
        }

        public static DhPublicKey ToDhPublicKey(this DhPublicKeyDataModel value)
        {
            return new DhPublicKey
            {
                Generator = value.Generator,
                Modulus = value.Modulus
            };
        }

        public static Chat ToChatDto(
            this ConversationDataModel value,
            string userId)
        { 
            return new Chat
            {
                Name = value.Name,
                Id = value.Id,
                DialogueUser = value.IsGroup ? null : value.GetDialogUser(userId)?.ToAppUserDto(),
                IsGroup = value.IsGroup,
                ThumbnailUrl = value.ThumbnailUrl,
                FullImageUrl = value.FullImageUrl,
                Participants = value.participants?.Select(x => x.ToAppUserDto()).ToList(),
                AuthKeyId = value.AuthKeyId,
                IsSecure = value.IsSecure,
                PublicKey = value.PublicKey?.ToDhPublicKey(),
                DeviceId = value.DeviceId,
                ChatRole = value.Role?.ToChatRole(),
                ClientLastMessageId = value.LastMessage?.MessageID ?? 0,
                LastMessage = value.LastMessage?.ToMessage(),
                IsPublic = value.IsPublic,
                IsMessagingRestricted = value.IsMessagingRestricted,
                MessagesUnread = value.UnreadCount
            };
        }

        public static Message ToMessage(this MessageDataModel value)
        {
            return new Message
            {
                Id = value.MessageID,
                ConversationID = value.ConversationID,
                MessageContent = value.MessageContent,
                TimeReceived = value.TimeReceived.ToUTCString(),
                User = value.User?.ToAppUserDto(),
                AttachmentInfo = value.AttachmentInfo?.ToMessageAttachment(),
                Type = value.Type,
                ForwardedMessage = value.ForwardedMessage?.ToMessage(),
                State = value.State,
                EncryptedPayload = value.EncryptedPayload,
                Event = value.Event?.ToChatEvent()
            };
        }

        public static ChatEvent ToChatEvent(this ChatEventDataModel value)
        {
            return new ChatEvent()
            {
                Actor = value.ActorId,
                Type = value.EventType,
                UserInvolved = value.UserInvolvedId,
                ActorName = value.Actor?.UserName,
                UserInvolvedName = value.UserInvolved?.UserName
            };
        }

        public static ChatRoleDto ToChatRole(this ChatRoleDataModel value)
        {
            return new ChatRoleDto
            {
                ChatId = value.ChatId,
                Role = value.RoleId
            };
        }
    }
}