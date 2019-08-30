using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public class AttachmentsRepository : BaseRepository<MessageAttachmentDataModel>, IAttachmentsRepository
    {
        public AttachmentsRepository(ApplicationDbContext db) : base(db)
        {

        }
    }
}
