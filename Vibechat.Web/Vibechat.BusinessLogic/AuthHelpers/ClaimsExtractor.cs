using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Vibechat.BusinessLogic.AuthHelpers
{
    public class ClaimsExtractor
    {
        public static string GetNamedClaimValue(IEnumerable<Claim> claims, string name)
        {
            return claims.FirstOrDefault(x => x.Type == name)?.Value;
        }

        public static string GetUserIdClaim(IEnumerable<Claim> claims)
        {
            return GetNamedClaimValue(claims, JwtTokenGenerator.JwtUserIdClaimName);
        }
    }
}