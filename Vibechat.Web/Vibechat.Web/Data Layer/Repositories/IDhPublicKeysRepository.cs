using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Data.Repositories
{
    public interface IDhPublicKeysRepository 
    {
        Task<DhPublicKeyDataModel> GetRandomKey();
    }
}