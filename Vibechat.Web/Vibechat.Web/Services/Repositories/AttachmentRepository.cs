using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Services.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private ApplicationDbContext mContext { get; set; }

        public AttachmentRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public MessageAttachmentDataModel Add(AttachmentKindDataModel attachmentKind, Message message)
        {
            var attachment = new MessageAttachmentDataModel()
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
            foreach (var attachment in attachments)
            {
                mContext.Attachments.Remove(attachment);
            }
        }
    }
}
