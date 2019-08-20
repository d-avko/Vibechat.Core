using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public class AttachmentsRepository : BaseRepository<MessageAttachmentDataModel>, IAttachmentsRepository
    {
        public AttachmentsRepository(ApplicationDbContext db) : base(db)
        {

        }
    }
}
