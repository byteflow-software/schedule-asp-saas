using Scheduly.Domain.Common;

namespace Scheduly.Domain.Entities;

public class Vacancy : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBooked { get; set; }
    public Guid? AppointmentId { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public User User { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public Appointment? Appointment { get; set; }
}
