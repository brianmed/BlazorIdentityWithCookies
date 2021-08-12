using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Tokens;

using IdentityWithCookies.Server.Entities;
using IdentityWithCookies.Shared;

namespace IdentityWithCookies.Server
{
    public class JwtToken
    {
        async static public Task<string> JwtSecretAsync()
        {
            return "0B46715D-661C-49D2-8BB7-692DD23F81C4";
        }

        async public static Task<string> JwtIssuerAsync()
        {
            return "localhost/";
        }

        async public static Task<string> GenerateAsync(AccountEntity account)
        {
            List<Claim> claims = new()
            {
                new(ClaimTypes.Name, account.AccountEntityId.ToString())
            };

            claims.Add(new Claim(ClaimTypes.Role, AuthorizationRoles.Admin.ToString()));

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(await JwtSecretAsync()));
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                await JwtIssuerAsync(),
                null,
                claims,
                expires: DateTime.Now.AddDays(14),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
