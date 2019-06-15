using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web;

namespace Vibechat.Web.Services.Repositories
{
    public interface IContactsRepository
    {
        Task AddContact(UserInApplication whoAdds, UserInApplication contact);
        IQueryable<ContactsDataModel> GetContactsOf(string id);
        Task RemoveContact(string whoRemovesId, string contactId);
    }
}