using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;
using Vibechat.Web.Data_Layer.Repositories;

namespace Vibechat.Web.Data.Repositories
{
    public interface IContactsRepository : IAsyncRepository<ContactsDataModel>
    {
        ValueTask<ContactsDataModel> GetByIdAsync(string user, string contactId);
    }
}