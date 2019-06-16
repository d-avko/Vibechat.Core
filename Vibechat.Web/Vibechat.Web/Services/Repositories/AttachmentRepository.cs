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

        public async Task<MessageAttachmentDataModel> Add(AttachmentKindDataModel attachmentKind, Message message)
        {
            var val = (await mContext.Attachments.AddAsync(new MessageAttachmentDataModel()
            {
                AttachmentKind = attachmentKind,
                ContentUrl = message.AttachmentInfo.ContentUrl,
                ImageHeight = message.AttachmentInfo.ImageHeight,
                ImageWidth = message.AttachmentInfo.ImageWidth,
                AttachmentName = message.AttachmentInfo.AttachmentName
            })).Entity;

            await mContext.SaveChangesAsync();

            return val;
        }

        public async Task Remove(List<MessageAttachmentDataModel> attachments)
        {
            attachments.ForEach((a) => mContext.Attachments.Remove(a));
            await mContext.SaveChangesAsync();
        }
    }
}
