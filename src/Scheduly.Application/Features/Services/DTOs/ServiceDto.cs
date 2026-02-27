using Scheduly.Domain.Entities;

namespace Scheduly.Application.Features.Services.DTOs;

public record ServiceDto(
    Guid Id,
    string Name,
    string? Description,
    int DurationMinutes,
    int PriceInCents,
    bool IsActive,
    DateTime CreatedAt)
{
    public static ServiceDto FromEntity(Service service) => new(
        service.Id,
        service.Name,
        service.Description,
        service.DurationMinutes,
        service.PriceInCents,
        service.IsActive,
        service.CreatedAt);
}
