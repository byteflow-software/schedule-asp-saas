using Scheduly.Domain.Entities;

namespace Scheduly.Application.Features.Vacancies.DTOs;

public record VacancyDto(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid ServiceId,
    string ServiceName,
    DateTime StartTime,
    DateTime EndTime,
    bool IsBooked,
    Guid? AppointmentId,
    DateTime CreatedAt)
{
    public static VacancyDto FromEntity(Vacancy v) => new(
        v.Id, v.UserId, v.User?.FullName ?? "", v.ServiceId, v.Service?.Name ?? "",
        v.StartTime, v.EndTime, v.IsBooked, v.AppointmentId, v.CreatedAt);
}
