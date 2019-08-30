using System.Threading.Tasks;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories
{
    public interface IDhPublicKeysRepository 
    {
        Task<DhPublicKeyDataModel> GetRandomKey();
    }
}