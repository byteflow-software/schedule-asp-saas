using MediatR;
using Scheduly.Application.Features.Appointments.DTOs;

namespace Scheduly.Application.Features.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand(
    Guid CustomerId,
    Guid UserId,
    Guid ServiceId,
    Guid? VacancyId,
    DateTime StartTime,
    DateTime EndTime,
    string? Notes) : IRequest<AppointmentDto>;
