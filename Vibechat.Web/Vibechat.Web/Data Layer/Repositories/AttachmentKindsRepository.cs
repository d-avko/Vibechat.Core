using System.Threading.Tasks;
using VibeChat.Web;
using VibeChat.Web.Data.DataModels;
using Vibechat.Web.Data.Messages;
using Vibechat.Web.Data_Layer.Repositories;

namespace Vibechat.Web.Data.Repositories
{
    public class AttachmentKindsRepository : BaseRepository<AttachmentKindDataModel>, IAttachmentKindsRepository
    {
        public AttachmentKindsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            
        }

        public ValueTask<AttachmentKindDataModel> GetById(AttachmentKind kind)
        {
            return _dbContext.AttachmentKinds.FindAsync(kind);
        }
    }
}