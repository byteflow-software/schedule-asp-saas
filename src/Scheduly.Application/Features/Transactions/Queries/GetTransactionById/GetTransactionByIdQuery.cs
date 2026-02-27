using MediatR;
using Scheduly.Application.Features.Transactions.DTOs;

namespace Scheduly.Application.Features.Transactions.Queries.GetTransactionById;

public record GetTransactionByIdQuery(Guid Id) : IRequest<TransactionDto>;
