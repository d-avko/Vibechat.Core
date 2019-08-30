using System.Threading.Tasks;

namespace Vibechat.BusinessLogic.AuthHelpers
{
    public interface ITokenValidator
    {
        Task<bool> Validate(string userId, string refreshToken);
    }
}