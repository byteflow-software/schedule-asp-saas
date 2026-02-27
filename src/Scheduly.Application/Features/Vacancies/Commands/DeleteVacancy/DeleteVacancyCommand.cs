using MediatR;

namespace Scheduly.Application.Features.Vacancies.Commands.DeleteVacancy;

public record DeleteVacancyCommand(Guid Id) : IRequest;
