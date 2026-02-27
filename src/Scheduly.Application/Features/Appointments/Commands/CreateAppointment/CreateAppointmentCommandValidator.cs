using FluentValidation;

namespace Scheduly.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.StartTime)
            .NotEmpty()
            .GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");
        RuleFor(x => x.EndTime)
            .NotEmpty()
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");
    }
}
