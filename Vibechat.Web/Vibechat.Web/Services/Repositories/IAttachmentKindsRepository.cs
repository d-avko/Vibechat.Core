using System.Threading.Tasks;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Services.Repositories
{
    public interface IAttachmentKindsRepository
    {
        Task<AttachmentKindDataModel> GetById(string id);
    }
}