namespace Scheduly.Domain.Entities;

public class ErrorLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Error details
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ExceptionType { get; set; }
    public string? StackTrace { get; set; }
    public string? Source { get; set; }

    // HTTP request context
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public string? RequestBody { get; set; }
    public int? HttpStatusCode { get; set; }

    // User/Tenant context (no FK, just reference)
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }

    // External API context (Asaas etc.)
    public string? ExternalRequestUrl { get; set; }
    public string? ExternalResponseBody { get; set; }
    public int? ExternalStatusCode { get; set; }
}
