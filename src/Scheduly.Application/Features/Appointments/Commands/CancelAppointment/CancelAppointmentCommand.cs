using MediatR;

namespace Scheduly.Application.Features.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand(Guid Id) : IRequest;
