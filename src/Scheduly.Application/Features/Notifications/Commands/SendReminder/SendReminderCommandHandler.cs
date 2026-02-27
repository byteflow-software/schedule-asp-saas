using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Notifications.Commands.SendReminder;

public class SendReminderCommandHandler : IRequestHandler<SendReminderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SendReminderCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _emailService = emailService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(SendReminderCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .IgnoreQueryFilters()
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken)
            ?? throw new EntityNotFoundException("Appointment", request.AppointmentId);

        await _emailService.SendReminderAsync(
            appointment.Customer.Email,
            appointment.Customer.FullName,
            appointment.StartTime,
            cancellationToken);

        appointment.ReminderSentAt = _dateTimeProvider.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
