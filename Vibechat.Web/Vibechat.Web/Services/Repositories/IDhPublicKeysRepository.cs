using System.Threading.Tasks;
using Vibechat.Web.Data.DataModels;

namespace Vibechat.Web.Services.Repositories
{
    public interface IDhPublicKeysRepository
    {
        Task Add(DhPublicKeyDataModel value);
    }
}