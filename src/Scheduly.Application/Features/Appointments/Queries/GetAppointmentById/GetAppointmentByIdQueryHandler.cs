using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Appointments.DTOs;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentDto>
{
    private readonly IApplicationDbContext _context;

    public GetAppointmentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AppointmentDto> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.User)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Appointment", request.Id);

        var transaction = await _context.Transactions
            .Include(t => t.Customer)
            .FirstOrDefaultAsync(t => t.AppointmentId == appointment.Id, cancellationToken);

        return AppointmentDto.FromEntity(appointment, transaction);
    }
}
