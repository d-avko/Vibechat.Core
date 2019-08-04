using System.Linq;
using VibeChat.Web;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Data.Repositories
{
    public class ContactsRepository : IContactsRepository
    {
        public ContactsRepository(ApplicationDbContext dbContext)
        {
            mContext = dbContext;
        }

        private ApplicationDbContext mContext { get; }

        public IQueryable<ContactsDataModel> GetContactsOf(string id)
        {
            return mContext.Contacts.Where(x => x.FirstUserID == id);
        }

        public void RemoveContact(string whoRemovesId, string contactId)
        {
            var contact =
                mContext.Contacts.FirstOrDefault(x => x.FirstUserID == whoRemovesId && x.SecondUserID == contactId);
            mContext.Contacts.Remove(contact);
        }

        public void AddContact(string whoAdds, string contact)
        {
            mContext.Contacts.Add(new ContactsDataModel {FirstUserID = whoAdds, SecondUserID = contact});
        }
    }
}