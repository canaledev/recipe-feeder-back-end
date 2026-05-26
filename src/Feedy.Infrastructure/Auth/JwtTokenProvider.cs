namespace Feedy.Infrastructure.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Feedy.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Generates HS256-signed JWT access tokens using settings from JwtSettings in appsettings.
/// </summary>
internal sealed class JwtTokenProvider : IJwtTokenProvider
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenProvider(IConfiguration configuration)
    {
        var section = configuration.GetSection("JwtSettings");
        _secret = section["Secret"]
            ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");
        _issuer = section["Issuer"] ?? "feedy-api";
        _audience = section["Audience"] ?? "feedy-frontend";
        _expirationMinutes = int.TryParse(section["ExpirationMinutes"], out var minutes) ? minutes : 60;
    }

    public string GenerateToken(Guid userId, string email, string fullName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("fullName", fullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
