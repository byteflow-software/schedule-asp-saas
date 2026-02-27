using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Appointments.DTOs;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Appointments.Commands.UpdateAppointment;

public class UpdateAppointmentCommandHandler : IRequestHandler<UpdateAppointmentCommand, AppointmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateAppointmentCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<AppointmentDto> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.User)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Appointment", request.Id);

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            throw new DomainException("INVALID_STATUS", "Cannot edit a cancelled or completed appointment.");

        // Check for time overlap on the same staff member (excluding current appointment)
        var hasOverlap = await _context.Appointments
            .AnyAsync(a =>
                a.Id != request.Id &&
                a.UserId == appointment.UserId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.StartTime < request.EndTime &&
                a.EndTime > request.StartTime,
                cancellationToken);

        if (hasOverlap)
            throw new AppointmentOverlapException();

        appointment.StartTime = request.StartTime;
        appointment.EndTime = request.EndTime;
        appointment.Notes = request.Notes;
        appointment.UpdatedAt = _dateTimeProvider.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var transaction = await _context.Transactions
            .Include(t => t.Customer)
            .FirstOrDefaultAsync(t => t.AppointmentId == appointment.Id, cancellationToken);

        return AppointmentDto.FromEntity(appointment, transaction);
    }
}
