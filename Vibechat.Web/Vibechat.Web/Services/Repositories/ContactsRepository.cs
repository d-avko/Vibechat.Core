using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using VibeChat.Web;

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
            return mContext.Contacts.Where(x => x.User.Id == id);
        }

        public async Task RemoveContact(string whoRemovesId, string contactId)
        {
            var contact = mContext.Contacts.FirstOrDefault(x => x.User.Id == whoRemovesId && x.Contact.Id == contactId);
            mContext.Contacts.Remove(contact);
            await mContext.SaveChangesAsync();
        }

        public async Task AddContact(UserInApplication whoAdds, UserInApplication contact)
        {
            mContext.Contacts.Add(new ContactsDataModel() { Contact = contact, User = whoAdds });
            await mContext.SaveChangesAsync();
        }
    }
}
