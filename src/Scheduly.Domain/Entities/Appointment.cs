using Scheduly.Domain.Common;
using Scheduly.Domain.Enums;

namespace Scheduly.Domain.Entities;

public class Appointment : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid UserId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid? VacancyId { get; set; }
    public int PriceInCents { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Notes { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.PendingPayment;
    public DateTime? ReminderSentAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public User User { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public Vacancy? Vacancy { get; set; }
    public Transaction? Transaction { get; set; }

    public bool OverlapsWith(DateTime start, DateTime end)
    {
        return StartTime < end && EndTime > start;
    }
}
