using MediatR;

namespace Scheduly.Application.Features.Transactions.Commands.CancelTransaction;

public record CancelTransactionCommand(Guid Id) : IRequest;
