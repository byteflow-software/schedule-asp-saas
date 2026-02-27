using Scheduly.Domain.Entities;

namespace Scheduly.Application.Features.Tenants.DTOs;

public record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    DateTime CreatedAt)
{
    public static TenantDto FromEntity(Tenant tenant) => new(
        tenant.Id,
        tenant.Name,
        tenant.Slug,
        tenant.IsActive,
        tenant.CreatedAt);
}
