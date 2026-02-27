using MediatR;
using Scheduly.Application.Features.Transactions.DTOs;

namespace Scheduly.Application.Features.Transactions.Commands.PayTransaction;

public record PayTransactionCommand(Guid Id, string PaymentMethod) : IRequest<TransactionDto>;
