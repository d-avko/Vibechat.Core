using System.Threading.Tasks;
using VibeChat.Web.Data.DataModels;
using Vibechat.Web.Data.Messages;

namespace Vibechat.Web.Data.Repositories
{
    public interface IAttachmentKindsRepository
    {
        ValueTask<AttachmentKindDataModel> GetById(AttachmentKind id);
    }
}