using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Appointments.Commands.ConfirmAppointment;

public class ConfirmAppointmentCommandHandler : IRequestHandler<ConfirmAppointmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ConfirmAppointmentCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(ConfirmAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Appointment", request.Id);

        if (appointment.Status != AppointmentStatus.PendingPayment)
            throw new DomainException("INVALID_STATUS", "Only pending payment appointments can be confirmed.");

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.UpdatedAt = _dateTimeProvider.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
