namespace Scheduly.Application.Features.Transactions.DTOs;

public record TransactionSummaryDto(
    int TotalRevenueCents,
    int TotalPendingCents,
    int TotalPaidCents,
    int Count);
