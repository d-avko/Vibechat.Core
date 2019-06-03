using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Vibechat.Web.Services.Repositories;

namespace Vibechat.Web.AuthHelpers
{
    public class JwtTokenValidator : ITokenValidator
    {
        public JwtTokenValidator(IUsersRepository usersRepository, JwtSecurityTokenHandler tokensHandler)
        {
            UsersRepository = usersRepository;
            this.tokensHandler = tokensHandler;
        }

        protected JwtSecurityTokenHandler tokensHandler { get; set; }
        public IUsersRepository UsersRepository { get; }

        public async Task<bool> Validate(string userId, string refreshToken)
        {
            string token = await UsersRepository.GetRefreshToken(userId);
            
            if(token != refreshToken)
            {
                return false;
            }

            JwtSecurityToken parsedToken = tokensHandler.ReadJwtToken(refreshToken);

            if(parsedToken.ValidTo < DateTime.Now)
            {
                return false;
            }

            return true;
        }
    }
}
