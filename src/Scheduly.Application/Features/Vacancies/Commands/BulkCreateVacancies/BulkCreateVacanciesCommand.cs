using MediatR;
using Scheduly.Application.Features.Vacancies.DTOs;

namespace Scheduly.Application.Features.Vacancies.Commands.BulkCreateVacancies;

public record SlotDto(DateTime StartTime, DateTime EndTime);

public record BulkCreateVacanciesCommand(
    Guid UserId,
    Guid ServiceId,
    List<SlotDto> Slots) : IRequest<List<VacancyDto>>;
