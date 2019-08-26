using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace VibeChat.Web
{
    public static class JwtHelper
    {
        public const string JwtUserIdClaimName = "UserId";

        /// <summary>
        ///     Generates jwt token and returns it as string
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GenerateRefreshToken(this AppUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                //add custom claim to identify the user by his id
                new Claim(JwtUserIdClaimName, user.Id)
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DI.Configuration["Jwt:SecretKey"] as string)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                DI.Configuration["Jwt:Issuer"],
                DI.Configuration["Jwt:Audience"],
                claims,
                signingCredentials: credentials,
                expires: DateTime.UtcNow.AddYears(1)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GenerateToken(this AppUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
                //add custom claim to identify the user by his id
                new Claim(JwtUserIdClaimName, user.Id)
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DI.Configuration["Jwt:SecretKey"] as string)),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                DI.Configuration["Jwt:Issuer"],
                DI.Configuration["Jwt:Audience"],
                claims,
                signingCredentials: credentials,
                expires: DateTime.UtcNow.AddMinutes(10)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GetNamedClaimValue(IEnumerable<Claim> claims, string name = JwtUserIdClaimName)
        {
            return claims.FirstOrDefault(x => x.Type == name)?.Value;
        }
    }
}