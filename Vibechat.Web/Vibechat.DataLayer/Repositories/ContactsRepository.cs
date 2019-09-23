using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public class ContactsRepository : BaseRepository<ContactsDataModel>, IContactsRepository
    {
        public ContactsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public ValueTask<ContactsDataModel> GetByIdAsync(string userId, string contactId)
        {
            return _dbContext.Contacts.FindAsync(userId, contactId);
        }
    }
}