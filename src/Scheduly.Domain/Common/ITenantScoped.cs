namespace Scheduly.Domain.Common;

public interface ITenantScoped
{
    Guid TenantId { get; set; }
}
