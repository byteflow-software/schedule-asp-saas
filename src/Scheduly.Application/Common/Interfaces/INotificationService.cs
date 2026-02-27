namespace Scheduly.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendReminderAsync(string customerEmail, string customerName, DateTime appointmentTime, CancellationToken cancellationToken = default);
}
