using System.Collections.Generic;
using VibeChat.Web;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Data.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        public AttachmentRepository(ApplicationDbContext dbContext)
        {
            mContext = dbContext;
        }

        private ApplicationDbContext mContext { get; }

        public MessageAttachmentDataModel Add(AttachmentKindDataModel attachmentKind, Message message)
        {
            var attachment = new MessageAttachmentDataModel
            {
                AttachmentKind = attachmentKind,
                ContentUrl = message.AttachmentInfo.ContentUrl,
                ImageHeight = message.AttachmentInfo.ImageHeight,
                ImageWidth = message.AttachmentInfo.ImageWidth,
                AttachmentName = message.AttachmentInfo.AttachmentName,
                FileSize = message.AttachmentInfo.FileSize
            };

            return mContext.Attachments.Add(attachment)?.Entity;
        }

        public void Remove(List<MessageAttachmentDataModel> attachments)
        {
            foreach (var attachment in attachments) mContext.Attachments.Remove(attachment);
        }
    }
}