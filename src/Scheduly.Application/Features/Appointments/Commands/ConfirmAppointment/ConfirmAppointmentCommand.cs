using MediatR;

namespace Scheduly.Application.Features.Appointments.Commands.ConfirmAppointment;

public record ConfirmAppointmentCommand(Guid Id) : IRequest;
