using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VibeChat.Web.ChatData;

namespace VibeChat.Web.Data.DataModels
{
    public class MessageAttachmentDataModel
    {
        [Key] public int AttachmentID { get; set; }

        public string ContentUrl { get; set; }

        public string AttachmentName { get; set; }

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public AttachmentKindDataModel AttachmentKind { get; set; }

        public MessageDataModel Message { get; set; }

        [ForeignKey("MessageId")] public int? MessageId { get; set; }

        public long FileSize { get; set; }

        public static MessageAttachmentDataModel Create(AttachmentKindDataModel attachmentKind, Message message)
        {
            return new MessageAttachmentDataModel
            {
                AttachmentKind = attachmentKind,
                ContentUrl = message.AttachmentInfo.ContentUrl,
                ImageHeight = message.AttachmentInfo.ImageHeight,
                ImageWidth = message.AttachmentInfo.ImageWidth,
                AttachmentName = message.AttachmentInfo.AttachmentName,
                FileSize = message.AttachmentInfo.FileSize
            };
        }
    }
}