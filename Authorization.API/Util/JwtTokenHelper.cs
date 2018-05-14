using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authorization.API.Util
{
    public static class JwtTokenHelper
    {
        public static String WriteJwtToken(IEnumerable<Claim> claims, String signingKey, int expirationTime)
        {
            var token = new JwtSecurityToken
            (
                claims: claims,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(signingKey)),
                        SecurityAlgorithms.HmacSha256),
                expires: DateTime.Now.AddMinutes(expirationTime),
                notBefore: DateTime.Now
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
