using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CancelAppointmentCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Appointment", request.Id);

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            throw new DomainException("INVALID_STATUS", "This appointment cannot be cancelled.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = now;

        // Cancel linked transaction
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.AppointmentId == appointment.Id && t.Status != TransactionStatus.Cancelled, cancellationToken);
        if (transaction != null)
        {
            transaction.Status = TransactionStatus.Cancelled;
            transaction.UpdatedAt = now;
        }

        // Free vacancy if booked
        if (appointment.VacancyId.HasValue)
        {
            var vacancy = await _context.Vacancies
                .FirstOrDefaultAsync(v => v.Id == appointment.VacancyId.Value, cancellationToken);
            if (vacancy != null)
            {
                vacancy.IsBooked = false;
                vacancy.AppointmentId = null;
                vacancy.UpdatedAt = now;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
