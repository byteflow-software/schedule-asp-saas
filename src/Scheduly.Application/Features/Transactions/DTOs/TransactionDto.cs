using Scheduly.Domain.Entities;

namespace Scheduly.Application.Features.Transactions.DTOs;

public record TransactionDto(
    Guid Id,
    Guid AppointmentId,
    Guid CustomerId,
    string CustomerName,
    string ReferenceNumber,
    int AmountInCents,
    string Status,
    string Type,
    string? Description,
    DateTime? PaidAt,
    string? PaymentMethod,
    DateTime CreatedAt)
{
    public static TransactionDto FromEntity(Transaction t) => new(
        t.Id, t.AppointmentId, t.CustomerId,
        t.Customer?.FullName ?? "",
        t.ReferenceNumber, t.AmountInCents,
        t.Status.ToString(), t.Type.ToString(),
        t.Description, t.PaidAt, t.PaymentMethod, t.CreatedAt);
}
