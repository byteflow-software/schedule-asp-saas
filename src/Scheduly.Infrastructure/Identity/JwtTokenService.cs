using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Common.Models;
using Scheduly.Domain.Entities;

namespace Scheduly.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly IDateTimeProvider _dateTimeProvider;

    public JwtTokenService(IOptions<JwtSettings> settings, IDateTimeProvider dateTimeProvider)
    {
        _settings = settings.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    public TokenResponse GenerateTokens(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
            [JwtRegisteredClaimNames.Email] = user.Email,
            ["tenant_id"] = user.TenantId.ToString(),
            [ClaimTypes.Role] = user.Role.ToString()
        };

        var now = _dateTimeProvider.UtcNow;
        var expiresAt = now.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Claims = claims,
            NotBefore = now,
            Expires = expiresAt,
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();
        var accessToken = handler.CreateToken(tokenDescriptor);

        var refreshToken = GenerateRefreshToken();

        return new TokenResponse(accessToken, refreshToken, expiresAt);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
