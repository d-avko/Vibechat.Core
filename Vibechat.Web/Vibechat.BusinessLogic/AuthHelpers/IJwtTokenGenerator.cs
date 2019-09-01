using Vibechat.DataLayer;

namespace Vibechat.BusinessLogic.AuthHelpers
{
    public interface IJwtTokenGenerator
    {
        string GenerateRefreshToken(AppUser user);
        string GenerateToken(AppUser user);
    }
}