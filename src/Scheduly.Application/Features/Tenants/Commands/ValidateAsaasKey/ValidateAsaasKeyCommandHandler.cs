using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Tenants.DTOs;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Tenants.Commands.ValidateAsaasKey;

public class ValidateAsaasKeyCommandHandler : IRequestHandler<ValidateAsaasKeyCommand, TenantDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IAsaasService _asaasService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ValidateAsaasKeyCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        IAsaasService asaasService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _asaasService = asaasService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TenantDto> Handle(ValidateAsaasKeyCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == _currentTenantService.TenantId, cancellationToken)
            ?? throw new EntityNotFoundException("Tenant", _currentTenantService.TenantId);

        var account = await _asaasService.ValidateApiKeyAsync(request.ApiKey, cancellationToken);

        tenant.AsaasApiKey = request.ApiKey;
        tenant.AsaasWalletId = account.WalletId;
        tenant.UpdatedAt = _dateTimeProvider.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return TenantDto.FromEntity(tenant);
    }
}
