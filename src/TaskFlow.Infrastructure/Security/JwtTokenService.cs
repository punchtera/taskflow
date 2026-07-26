using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Security;

/// <summary>
/// Issues signed JWTs (HMAC-SHA256) carrying the user's id, email and name.
/// The same signing key/issuer/audience are validated by the API's bearer handler.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly TimeProvider _clock;

    public JwtTokenService(JwtSettings settings, TimeProvider clock)
    {
        _settings = settings;
        _clock = clock;
    }

    public (string Token, DateTime ExpiresAtUtc) CreateToken(User user)
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(_settings.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        return (encoded, expires);
    }
}
