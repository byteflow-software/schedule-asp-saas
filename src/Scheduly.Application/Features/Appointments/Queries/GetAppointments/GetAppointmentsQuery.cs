using MediatR;
using Scheduly.Application.Common.Models;
using Scheduly.Application.Features.Appointments.DTOs;

namespace Scheduly.Application.Features.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery(
    DateTime? From = null,
    DateTime? To = null,
    Guid? UserId = null,
    Guid? CustomerId = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<AppointmentDto>>;
