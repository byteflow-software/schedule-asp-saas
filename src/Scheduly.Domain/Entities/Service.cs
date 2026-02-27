using Scheduly.Domain.Common;

namespace Scheduly.Domain.Entities;

public class Service : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public int PriceInCents { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<Vacancy> Vacancies { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}
