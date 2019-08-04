using System.Threading.Tasks;
using VibeChat.Web;
using VibeChat.Web.Data.DataModels;
using Vibechat.Web.Data.Messages;

namespace Vibechat.Web.Data.Repositories
{
    public class AttachmentKindsRepository : IAttachmentKindsRepository
    {
        public AttachmentKindsRepository(ApplicationDbContext dbContext)
        {
            mContext = dbContext;
        }

        private ApplicationDbContext mContext { get; }

        public Task<AttachmentKindDataModel> GetById(AttachmentKind kind)
        {
            return mContext.AttachmentKinds.FindAsync(kind);
        }
    }
}