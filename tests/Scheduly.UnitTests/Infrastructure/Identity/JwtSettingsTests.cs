using FluentAssertions;
using Scheduly.Infrastructure.Identity;

namespace Scheduly.UnitTests.Infrastructure.Identity;

public class JwtSettingsTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var settings = new JwtSettings();

        settings.Secret.Should().BeEmpty();
        settings.Issuer.Should().Be("Scheduly");
        settings.Audience.Should().Be("Scheduly");
        settings.AccessTokenExpirationMinutes.Should().Be(15);
        settings.RefreshTokenExpirationDays.Should().Be(7);
        JwtSettings.SectionName.Should().Be("Jwt");
    }
}
