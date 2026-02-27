using MediatR;
using Scheduly.Application.Features.Transactions.DTOs;

namespace Scheduly.Application.Features.Transactions.Queries.GetTransactionSummary;

public record GetTransactionSummaryQuery(DateTime? From = null, DateTime? To = null) : IRequest<TransactionSummaryDto>;
