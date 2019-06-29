using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web;
using VibeChat.Web.Services.Repositories;

namespace Vibechat.Web.Services.Repositories
{
    public class ContactsRepository : IContactsRepository
    {
        private ApplicationDbContext mContext { get; set; }

        public ContactsRepository(ApplicationDbContext dbContext)
        {
            this.mContext = dbContext;
        }

        public IQueryable<ContactsDataModel> GetContactsOf(string id)
        {
            return mContext.Contacts.Where(x => x.FirstUserID == id);
        }

        public async Task RemoveContact(string whoRemovesId, string contactId)
        {
            var contact = mContext.Contacts.FirstOrDefault(x => x.FirstUserID == whoRemovesId && x.SecondUserID == contactId);
            mContext.Contacts.Remove(contact);
            await mContext.SaveChangesAsync();
        }

        public async Task AddContact(string whoAdds, string contact)
        {
            mContext.Contacts.Add(new ContactsDataModel() { FirstUserID = whoAdds, SecondUserID = contact});
            await mContext.SaveChangesAsync();
        }
    }
}
