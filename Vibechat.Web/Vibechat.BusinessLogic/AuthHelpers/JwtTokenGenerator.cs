using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Vibechat.DataLayer;

namespace Vibechat.BusinessLogic.AuthHelpers
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        public const string JwtUserIdClaimName = "UserId";

        private readonly IConfiguration config;

        public JwtTokenGenerator(IConfiguration config)
        {
            this.config = config;
        }
        
        /// <summary>
        ///     Generates jwt token and returns it as string
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string GenerateRefreshToken(AppUser user)
         {
             var claims = new[]
             {
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                 new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                 //add custom claim to identify the user by his id
                 new Claim(JwtUserIdClaimName, user.Id)
             };
 
             var credentials = new SigningCredentials(
                 new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"] as string)),
                 SecurityAlgorithms.HmacSha256
             );
 
             var token = new JwtSecurityToken(
                 config["Jwt:Issuer"],
                 config["Jwt:Audience"],
                 claims,
                 signingCredentials: credentials,
                 expires: DateTime.UtcNow.AddYears(1)
             );
 
             return new JwtSecurityTokenHandler().WriteToken(token);
         }
         
         public string GenerateToken(AppUser user)
         {
             var claims = new[]
             {
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                 new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                 //add custom claim to identify the user by his id
                 new Claim(JwtUserIdClaimName, user.Id)
             };
 
             var credentials = new SigningCredentials(
                 new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"] as string)),
                 SecurityAlgorithms.HmacSha256
             );
 
             var token = new JwtSecurityToken(
                 config["Jwt:Issuer"],
                 config["Jwt:Audience"],
                 claims,
                 signingCredentials: credentials,
                 expires: DateTime.UtcNow.AddMinutes(10)
             );
 
             return new JwtSecurityTokenHandler().WriteToken(token);
         }
    }
}