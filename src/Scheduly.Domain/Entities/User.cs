using Scheduly.Domain.Common;
using Scheduly.Domain.Enums;

namespace Scheduly.Domain.Entities;

public class User : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Staff;
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
}
