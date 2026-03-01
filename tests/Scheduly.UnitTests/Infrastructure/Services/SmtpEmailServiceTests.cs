using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Infrastructure.Services;

public class SmtpEmailServiceTests
{
    private readonly SmtpEmailService _sut;

    public SmtpEmailServiceTests()
    {
        var settings = Options.Create(new EmailSettings
        {
            Host = "localhost",
            Port = 0, // Invalid port to force failure
            From = "test@scheduly.com",
            FromName = "Scheduly Test",
            EnableSsl = false,
            Username = "user",
            Password = "pass"
        });

        _sut = new SmtpEmailService(settings, NullLogger<SmtpEmailService>.Instance);
    }

    [Fact]
    public async Task SendChargeEmailAsync_SmtpUnavailable_DoesNotThrow()
    {
        // SMTP is not running, but the service catches exceptions internally
        var act = () => _sut.SendChargeEmailAsync(
            "customer@test.com", "John Doe", 5000,
            "REF-001", DateTime.UtcNow.AddDays(1), "Check-up");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendReminderAsync_SmtpUnavailable_DoesNotThrow()
    {
        var act = () => _sut.SendReminderAsync(
            "customer@test.com", "John Doe", DateTime.UtcNow.AddHours(1));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendChargeEmailAsync_WithoutCredentials_DoesNotThrow()
    {
        var settings = Options.Create(new EmailSettings
        {
            Host = "localhost",
            Port = 0,
            From = "test@scheduly.com",
            FromName = "Scheduly",
            EnableSsl = false,
            Username = null,
            Password = null
        });

        var sut = new SmtpEmailService(settings, NullLogger<SmtpEmailService>.Instance);

        var act = () => sut.SendChargeEmailAsync(
            "customer@test.com", "Jane", 3000,
            "REF-002", DateTime.UtcNow, "Service");

        await act.Should().NotThrowAsync();
    }
}
