using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Tenants.DTOs;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Tenants.Queries.GetTenant;

public class GetTenantQueryHandler : IRequestHandler<GetTenantQuery, TenantDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;

    public GetTenantQueryHandler(IApplicationDbContext context, ICurrentTenantService currentTenantService)
    {
        _context = context;
        _currentTenantService = currentTenantService;
    }

    public async Task<TenantDto> Handle(GetTenantQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == _currentTenantService.TenantId, cancellationToken)
            ?? throw new EntityNotFoundException("Tenant", _currentTenantService.TenantId);

        return TenantDto.FromEntity(tenant);
    }
}
