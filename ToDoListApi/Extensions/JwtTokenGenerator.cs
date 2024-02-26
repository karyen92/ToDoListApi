using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ToDoListApi.Domains;

namespace ToDoListApi.Extensions;

public static class JwtTokenGenerator
{
    public static string GenerateUserToken(string key, string issuer, User user)
    {
        var symKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

        var claims = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        });

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = issuer,
            Audience = issuer,
            SigningCredentials = new SigningCredentials(symKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
}