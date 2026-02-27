using MediatR;
using Scheduly.Application.Common.Models;
using Scheduly.Application.Features.Transactions.DTOs;

namespace Scheduly.Application.Features.Transactions.Queries.GetTransactions;

public record GetTransactionsQuery(
    DateTime? From = null,
    DateTime? To = null,
    string? Status = null,
    Guid? CustomerId = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<TransactionDto>>;
