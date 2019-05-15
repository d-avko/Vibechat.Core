using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Services.Repositories
{
    public class AttachmentKindsRepository : IAttachmentKindsRepository
    {
        private ApplicationDbContext mContext { get; set; }

        public AttachmentKindsRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public async Task<AttachmentKindDataModel> GetById(string id)
        {
            return await mContext.AttachmentKinds.FindAsync(id);
        }
    }
}
