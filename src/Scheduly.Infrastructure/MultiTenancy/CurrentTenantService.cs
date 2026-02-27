using Scheduly.Application.Common.Interfaces;

namespace Scheduly.Infrastructure.MultiTenancy;

public class CurrentTenantService : ICurrentTenantService
{
    public Guid TenantId { get; private set; }

    public void SetTenantId(Guid tenantId) => TenantId = tenantId;
}
