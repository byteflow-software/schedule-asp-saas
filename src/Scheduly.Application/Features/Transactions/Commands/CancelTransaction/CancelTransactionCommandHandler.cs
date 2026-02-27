using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Transactions.Commands.CancelTransaction;

public class CancelTransactionCommandHandler : IRequestHandler<CancelTransactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CancelTransactionCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(CancelTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Transaction", request.Id);

        if (transaction.Status == TransactionStatus.Cancelled)
            throw new DomainException("ALREADY_CANCELLED", "Transaction is already cancelled.");

        transaction.Status = TransactionStatus.Cancelled;
        transaction.UpdatedAt = _dateTimeProvider.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
