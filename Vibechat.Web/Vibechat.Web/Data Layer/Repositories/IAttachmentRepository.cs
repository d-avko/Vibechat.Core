using System.Collections.Generic;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Data.Repositories
{
    public interface IAttachmentRepository
    {
        MessageAttachmentDataModel Add(AttachmentKindDataModel attachmentKind, Message message);

        void Remove(List<MessageAttachmentDataModel> attachments);
    }
}