using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VibeChat.Web.Data.DataModels;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public interface IAttachmentsRepository : IAsyncRepository<MessageAttachmentDataModel>
    {

    }
}
