using FluentValidation;

namespace Scheduly.Application.Features.Vacancies.Commands.CreateVacancy;

public class CreateVacancyCommandValidator : AbstractValidator<CreateVacancyCommand>
{
    public CreateVacancyCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty().GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");
    }
}
