using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface IContactsRepository : IAsyncRepository<ContactsDataModel>
    {
        Task<ContactsDataModel> GetByIdAsync(string user, string contactId);
    }
}