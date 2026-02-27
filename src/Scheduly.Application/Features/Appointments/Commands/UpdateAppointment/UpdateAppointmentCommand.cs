using MediatR;
using Scheduly.Application.Features.Appointments.DTOs;

namespace Scheduly.Application.Features.Appointments.Commands.UpdateAppointment;

public record UpdateAppointmentCommand(
    Guid Id,
    DateTime StartTime,
    DateTime EndTime,
    string? Notes) : IRequest<AppointmentDto>;
