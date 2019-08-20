using System.Linq;
using VibeChat.Web;
using Vibechat.Web.Data.DataModels;
using Vibechat.Web.Data_Layer.Repositories;
using System.Threading.Tasks;

namespace Vibechat.Web.Data.Repositories
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