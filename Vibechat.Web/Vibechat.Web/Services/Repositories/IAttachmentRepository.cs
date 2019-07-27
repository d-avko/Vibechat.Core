using System.Collections.Generic;
using System.Threading.Tasks;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Services.Repositories
{
    public interface IAttachmentRepository
    {
        MessageAttachmentDataModel Add(AttachmentKindDataModel attachmentKind, Message message);

        void Remove(List<MessageAttachmentDataModel> attachments);
    }
}