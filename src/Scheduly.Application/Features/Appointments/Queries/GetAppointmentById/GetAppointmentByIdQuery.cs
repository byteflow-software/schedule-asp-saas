using MediatR;
using Scheduly.Application.Features.Appointments.DTOs;

namespace Scheduly.Application.Features.Appointments.Queries.GetAppointmentById;

public record GetAppointmentByIdQuery(Guid Id) : IRequest<AppointmentDto>;
