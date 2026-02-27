using MediatR;

namespace Scheduly.Application.Features.Notifications.Commands.SendReminder;

public record SendReminderCommand(Guid AppointmentId) : IRequest;
