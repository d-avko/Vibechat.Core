using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Data.Repositories
{
    public interface IDhPublicKeysRepository
    {
        Task Add(DhPublicKeyDataModel value);

        Task<DhPublicKeyDataModel> GetRandomKey();
    }
}