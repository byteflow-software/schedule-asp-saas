using Scheduly.Domain.Common;

namespace Scheduly.Domain.Entities;

public class Customer : AuditableEntity, ITenantScoped, ISoftDeletable
{
    public Guid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = [];
}
