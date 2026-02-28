using Scheduly.Domain.Entities;

namespace Scheduly.Application.Features.Tenants.DTOs;

public record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    string? CpfCnpj,
    string? Phone,
    string? Email,
    string? Address,
    string? AddressNumber,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State,
    string? PostalCode,
    string? LogoUrl,
    string? AsaasWalletId,
    bool HasAsaasIntegration,
    DateTime CreatedAt)
{
    public static TenantDto FromEntity(Tenant tenant) => new(
        tenant.Id,
        tenant.Name,
        tenant.Slug,
        tenant.IsActive,
        tenant.CpfCnpj,
        tenant.Phone,
        tenant.Email,
        tenant.Address,
        tenant.AddressNumber,
        tenant.Complement,
        tenant.Neighborhood,
        tenant.City,
        tenant.State,
        tenant.PostalCode,
        tenant.LogoUrl,
        tenant.AsaasWalletId,
        !string.IsNullOrEmpty(tenant.AsaasApiKey),
        tenant.CreatedAt);
}
