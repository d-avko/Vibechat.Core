using System.Threading.Tasks;
using VibeChat.Web.Data.DataModels;
using Vibechat.Web.Data.Messages;
using Vibechat.Web.Data_Layer.Repositories;

namespace Vibechat.Web.Data.Repositories
{
    public interface IAttachmentKindsRepository : IAsyncRepository<AttachmentKindDataModel>
    {
        Task<AttachmentKindDataModel> GetById(AttachmentKind id);
    }
}