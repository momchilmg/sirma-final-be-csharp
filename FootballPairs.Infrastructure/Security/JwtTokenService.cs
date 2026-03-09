using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FootballPairs.Application.Auth;
using FootballPairs.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FootballPairs.Infrastructure.Security;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    private const int AccessTokenHours = 12;

    public string CreateAccessToken(User user)
    {
        var key = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("JWT signing key is not configured.");
        }

        var issuer = configuration["Jwt:Issuer"] ?? "FootballPairs.Api";
        var audience = configuration["Jwt:Audience"] ?? "FootballPairs.Client";
        var usernameClaimType = configuration["Jwt:UsernameClaimType"] ?? JwtRegisteredClaimNames.UniqueName;
        var roleClaimType = configuration["Jwt:RoleClaimType"] ?? ClaimTypes.Role;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(usernameClaimType, user.Username),
            new Claim(roleClaimType, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: now.AddHours(AccessTokenHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
