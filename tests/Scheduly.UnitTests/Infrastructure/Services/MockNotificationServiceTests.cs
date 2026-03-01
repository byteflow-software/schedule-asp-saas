using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Infrastructure.Services;

public class MockNotificationServiceTests
{
    [Fact]
    public async Task SendReminderAsync_CompletesSuccessfully()
    {
        var sut = new MockNotificationService(NullLogger<MockNotificationService>.Instance);

        var act = () => sut.SendReminderAsync(
            "customer@test.com", "John Doe", DateTime.UtcNow.AddHours(1));

        await act.Should().NotThrowAsync();
    }
}
