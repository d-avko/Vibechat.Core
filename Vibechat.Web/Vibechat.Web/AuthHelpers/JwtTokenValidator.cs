using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Vibechat.Web.AuthHelpers
{
    public class JwtTokenClaimValidator : ITokenClaimValidator
    {
        protected JwtSecurityTokenHandler tokensHandler { get; set; }

        public JwtTokenClaimValidator(JwtSecurityTokenHandler tokensHandler)
        {
            this.tokensHandler = tokensHandler;
        }

        public bool Validate(string token, string ClaimName, string ClaimValue)
        {
            JwtSecurityToken parsedToken = tokensHandler.ReadJwtToken(token);

            if(parsedToken.Claims.FirstOrDefault(x => (x.Type == ClaimName) && (x.Value == ClaimValue)) == default(Claim))
            {
                return false;
            }

            return true;
        }
    }
}
