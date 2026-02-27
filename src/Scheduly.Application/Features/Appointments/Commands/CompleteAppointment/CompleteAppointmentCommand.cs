using MediatR;

namespace Scheduly.Application.Features.Appointments.Commands.CompleteAppointment;

public record CompleteAppointmentCommand(Guid Id) : IRequest;
