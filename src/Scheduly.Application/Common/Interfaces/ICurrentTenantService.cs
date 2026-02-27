namespace Scheduly.Application.Common.Interfaces;

public interface ICurrentTenantService
{
    Guid TenantId { get; }
    void SetTenantId(Guid tenantId);
}
