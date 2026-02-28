using MediatR;
using Scheduly.Application.Features.Tenants.DTOs;

namespace Scheduly.Application.Features.Tenants.Commands.UpdateTenant;

public record UpdateTenantCommand(
    string Name,
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
    string? LogoUrl) : IRequest<TenantDto>;
