using System.Threading.Tasks;
using VibeChat.Web.ChatData;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Services.Repositories
{
    public interface IAttachmentRepository
    {
        Task<MessageAttachmentDataModel> Add(AttachmentKindDataModel attachmentKind, Message message);
    }
}