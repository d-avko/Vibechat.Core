using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Vibechat.DataLayer.Repositories;

namespace Vibechat.BusinessLogic.AuthHelpers
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
            var user = await UsersRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return false;
            }
            
            if (user.RefreshToken != refreshToken)
            {
                return false;
            }

            return true;
        }
    }
}