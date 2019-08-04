using System.Threading.Tasks;

namespace Vibechat.Web.AuthHelpers
{
    public interface ITokenValidator
    {
        Task<bool> Validate(string userId, string refreshToken);
    }
}