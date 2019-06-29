using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;

namespace VibeChat.Web.Services.Repositories
{
    public interface IContactsRepository
    {
        Task AddContact(string whoAdds, string contact);
        IQueryable<ContactsDataModel> GetContactsOf(string id);
        Task RemoveContact(string whoRemovesId, string contactId);
    }
}