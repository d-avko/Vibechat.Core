using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.Messages;
using VibeChat.Web;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Data.Repositories
{
    public class AttachmentKindsRepository : IAttachmentKindsRepository
    {
        private ApplicationDbContext mContext { get; set; }

        public AttachmentKindsRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public Task<AttachmentKindDataModel> GetById(AttachmentKind kind)
        {
            return mContext.AttachmentKinds.FindAsync(kind);
        }
    }
}
