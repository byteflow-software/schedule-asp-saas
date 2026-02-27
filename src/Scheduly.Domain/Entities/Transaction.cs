using Scheduly.Domain.Common;
using Scheduly.Domain.Enums;

namespace Scheduly.Domain.Entities;

public class Transaction : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid CustomerId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public int AmountInCents { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public TransactionType Type { get; set; } = TransactionType.Charge;
    public string? Description { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Appointment Appointment { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
}
