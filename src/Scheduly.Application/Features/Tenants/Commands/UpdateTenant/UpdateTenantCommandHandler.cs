using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Tenants.DTOs;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Tenants.Commands.UpdateTenant;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, TenantDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateTenantCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TenantDto> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == _currentTenantService.TenantId, cancellationToken)
            ?? throw new EntityNotFoundException("Tenant", _currentTenantService.TenantId);

        tenant.Name = request.Name;
        tenant.CpfCnpj = request.CpfCnpj;
        tenant.Phone = request.Phone;
        tenant.Email = request.Email;
        tenant.Address = request.Address;
        tenant.AddressNumber = request.AddressNumber;
        tenant.Complement = request.Complement;
        tenant.Neighborhood = request.Neighborhood;
        tenant.City = request.City;
        tenant.State = request.State;
        tenant.PostalCode = request.PostalCode;
        tenant.LogoUrl = request.LogoUrl;
        tenant.UpdatedAt = _dateTimeProvider.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return TenantDto.FromEntity(tenant);
    }
}
