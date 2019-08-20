using System.Linq;
using Vibechat.Web.Data_Layer.Repositories;
using VibeChat.Web;

namespace Vibechat.Web.Data.Repositories
{
    public interface IConversationRepository : IAsyncRepository<ConversationDataModel>
    { 

    }
}