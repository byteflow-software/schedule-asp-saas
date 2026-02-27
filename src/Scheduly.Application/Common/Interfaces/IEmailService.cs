namespace Scheduly.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendChargeEmailAsync(
        string customerEmail,
        string customerName,
        int amountInCents,
        string referenceNumber,
        DateTime appointmentDate,
        string serviceName,
        CancellationToken cancellationToken = default);

    Task SendReminderAsync(
        string customerEmail,
        string customerName,
        DateTime appointmentTime,
        CancellationToken cancellationToken = default);
}
