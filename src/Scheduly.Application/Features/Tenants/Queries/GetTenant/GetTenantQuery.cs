using MediatR;
using Scheduly.Application.Features.Tenants.DTOs;

namespace Scheduly.Application.Features.Tenants.Queries.GetTenant;

public record GetTenantQuery : IRequest<TenantDto>;
