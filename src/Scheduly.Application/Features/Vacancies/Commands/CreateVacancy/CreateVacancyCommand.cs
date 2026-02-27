using MediatR;
using Scheduly.Application.Features.Vacancies.DTOs;

namespace Scheduly.Application.Features.Vacancies.Commands.CreateVacancy;

public record CreateVacancyCommand(
    Guid UserId,
    Guid ServiceId,
    DateTime StartTime,
    DateTime EndTime) : IRequest<VacancyDto>;
