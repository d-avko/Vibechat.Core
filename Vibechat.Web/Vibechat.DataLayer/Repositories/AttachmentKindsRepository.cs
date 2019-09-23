using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;
using Vibechat.Shared.DTO.Messages;

namespace Vibechat.DataLayer.Repositories
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