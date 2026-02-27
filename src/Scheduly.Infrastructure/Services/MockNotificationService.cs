using Microsoft.Extensions.Logging;
using Scheduly.Application.Common.Interfaces;

namespace Scheduly.Infrastructure.Services;

public class MockNotificationService : INotificationService
{
    private readonly ILogger<MockNotificationService> _logger;

    public MockNotificationService(ILogger<MockNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendReminderAsync(string customerEmail, string customerName, DateTime appointmentTime, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[MOCK] Reminder sent to {CustomerName} ({Email}) for appointment at {Time}",
            customerName, customerEmail, appointmentTime);

        return Task.CompletedTask;
    }
}
