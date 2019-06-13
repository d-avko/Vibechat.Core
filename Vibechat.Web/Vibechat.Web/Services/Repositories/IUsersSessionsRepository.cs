using System.Linq;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IUsersSessionsRepository
    {
        IQueryable<SessionDataModel> GetSessions(UserInApplication user);
    }
}