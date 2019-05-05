using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibechat.Web.AuthHelpers
{
    public interface ITokenClaimValidator
    {
        bool Validate(string token, string ClaimName, string ClaimValue);
    }
}
