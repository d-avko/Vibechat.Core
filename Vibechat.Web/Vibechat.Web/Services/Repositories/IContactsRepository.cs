using System.Linq;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Services.Repositories
{
    public interface IContactsRepository
    {
        void AddContact(string whoAdds, string contact);
        IQueryable<ContactsDataModel> GetContactsOf(string id);
        void RemoveContact(string whoRemovesId, string contactId);
    }
}