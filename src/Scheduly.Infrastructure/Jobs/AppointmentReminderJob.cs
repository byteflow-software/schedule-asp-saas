using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Notifications.Commands.SendReminder;
using Scheduly.Domain.Enums;

namespace Scheduly.Infrastructure.Jobs;

public class AppointmentReminderJob
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<AppointmentReminderJob> _logger;

    public AppointmentReminderJob(
        IApplicationDbContext context,
        IMediator mediator,
        IDateTimeProvider dateTimeProvider,
        ILogger<AppointmentReminderJob> logger)
    {
        _context = context;
        _mediator = mediator;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var now = _dateTimeProvider.UtcNow;
        var reminderWindow = now.AddMinutes(30);

        var appointments = await _context.Appointments
            .IgnoreQueryFilters()
            .Where(a => (a.Status == AppointmentStatus.PendingPayment || a.Status == AppointmentStatus.Confirmed) &&
                        a.StartTime >= now &&
                        a.StartTime <= reminderWindow &&
                        a.ReminderSentAt == null)
            .ToListAsync();

        _logger.LogInformation("Found {Count} appointments needing reminders", appointments.Count);

        foreach (var appointment in appointments)
        {
            try
            {
                await _mediator.Send(new SendReminderCommand(appointment.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder for appointment {Id}", appointment.Id);
            }
        }
    }
}
