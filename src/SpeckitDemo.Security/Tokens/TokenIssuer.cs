using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SpeckitDemo.Security.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace SpeckitDemo.Security.Tokens;

public sealed class TokenIssuer : ITokenIssuer
{
    private readonly IConfiguration _configuration;

    public TokenIssuer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) Issue(int userId, string username, string email)
    {
        var key = Required("Jwt:Key");
        var issuer = Required("Jwt:Issuer");
        var audience = Required("Jwt:Audience");
        var expiryHours = int.Parse(_configuration["Jwt:ExpiryHours"] ?? "1");
        var expiresAt = DateTime.UtcNow.AddHours(expiryHours);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, username),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            expires: expiresAt,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private string Required(string name) =>
        _configuration[name] ?? throw new InvalidOperationException($"{name} configuration is missing.");
}
