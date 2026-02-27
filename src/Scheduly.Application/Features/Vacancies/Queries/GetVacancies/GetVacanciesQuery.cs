using MediatR;
using Scheduly.Application.Features.Vacancies.DTOs;

namespace Scheduly.Application.Features.Vacancies.Queries.GetVacancies;

public record GetVacanciesQuery(
    Guid? UserId = null,
    Guid? ServiceId = null,
    DateTime? From = null,
    DateTime? To = null,
    bool? AvailableOnly = null) : IRequest<List<VacancyDto>>;
