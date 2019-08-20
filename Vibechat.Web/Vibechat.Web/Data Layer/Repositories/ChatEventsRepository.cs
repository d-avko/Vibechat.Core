using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data_Layer.DataModels;
using VibeChat.Web;

namespace Vibechat.Web.Data_Layer.Repositories
{
    public class ChatEventsRepository : BaseRepository<ChatEventDataModel>, IChatEventsRepository
    {
        public ChatEventsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
