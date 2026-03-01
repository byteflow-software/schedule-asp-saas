using FluentAssertions;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Infrastructure.Services;

public class SettingsTests
{
    [Fact]
    public void AsaasSettings_Defaults()
    {
        var settings = new AsaasSettings();

        settings.BaseUrl.Should().Be("https://api.asaas.com/v3");
        settings.PlatformApiKey.Should().BeEmpty();
        settings.PlatformWalletId.Should().BeEmpty();
        settings.PlatformSplitPercent.Should().Be(3);
        settings.WebhookToken.Should().BeEmpty();
        AsaasSettings.SectionName.Should().Be("Asaas");
    }

    [Fact]
    public void EmailSettings_Defaults()
    {
        var settings = new EmailSettings();

        settings.Host.Should().Be("localhost");
        settings.Port.Should().Be(1025);
        settings.From.Should().Be("noreply@scheduly.com");
        settings.FromName.Should().Be("Scheduly");
        settings.EnableSsl.Should().BeFalse();
        settings.Username.Should().BeNull();
        settings.Password.Should().BeNull();
        EmailSettings.SectionName.Should().Be("Email");
    }
}
