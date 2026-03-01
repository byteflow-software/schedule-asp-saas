using FluentAssertions;
using Microsoft.Extensions.Options;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.Identity;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Infrastructure.Identity;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        var settings = Options.Create(new JwtSettings
        {
            Secret = "ThisIsASecretKeyForTestingThatMustBeAtLeast32Chars!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        });

        _sut = new JwtTokenService(settings, new DateTimeProvider());
    }

    [Fact]
    public void GenerateTokens_ReturnsValidTokenResponse()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "user@test.com",
            FullName = "Test User",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = _sut.GenerateTokens(user);

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateTokens_RefreshTokensAreDifferentEachTime()
    {
        var user = new User
        {
            Id = Guid.NewGuid(), TenantId = Guid.NewGuid(),
            Email = "user@test.com", FullName = "Test",
            PasswordHash = "hash", Role = UserRole.Staff,
            IsActive = true, CreatedAt = DateTime.UtcNow
        };

        var result1 = _sut.GenerateTokens(user);
        var result2 = _sut.GenerateTokens(user);

        result1.RefreshToken.Should().NotBe(result2.RefreshToken);
    }
}
