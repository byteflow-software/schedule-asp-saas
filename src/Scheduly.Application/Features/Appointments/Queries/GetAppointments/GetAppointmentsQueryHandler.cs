using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Common.Models;
using Scheduly.Application.Features.Appointments.DTOs;

namespace Scheduly.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, PaginatedList<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AppointmentDto>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.User)
            .Include(a => a.Service)
            .AsQueryable();

        if (request.From.HasValue)
            query = query.Where(a => a.StartTime >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(a => a.StartTime <= request.To.Value);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId.Value);

        if (request.CustomerId.HasValue)
            query = query.Where(a => a.CustomerId == request.CustomerId.Value);

        var projected = query
            .OrderBy(a => a.StartTime)
            .Select(a => new AppointmentDto(
                a.Id, a.CustomerId, a.Customer.FullName,
                a.UserId, a.User.FullName,
                a.ServiceId, a.Service != null ? a.Service.Name : "",
                a.VacancyId, a.PriceInCents,
                a.StartTime, a.EndTime, a.Notes,
                a.Status.ToString(), a.CreatedAt,
                null));

        return await PaginatedList<AppointmentDto>.CreateAsync(
            projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
