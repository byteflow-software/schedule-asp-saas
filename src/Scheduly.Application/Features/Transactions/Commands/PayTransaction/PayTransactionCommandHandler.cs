using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Transactions.DTOs;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Transactions.Commands.PayTransaction;

public class PayTransactionCommandHandler : IRequestHandler<PayTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PayTransactionCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TransactionDto> Handle(PayTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .Include(t => t.Customer)
            .Include(t => t.Appointment)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Transaction", request.Id);

        if (transaction.Status != TransactionStatus.Pending)
            throw new DomainException("INVALID_STATUS", "Only pending transactions can be paid.");

        transaction.Status = TransactionStatus.Paid;
        transaction.PaidAt = _dateTimeProvider.UtcNow;
        transaction.PaymentMethod = request.PaymentMethod;
        transaction.UpdatedAt = _dateTimeProvider.UtcNow;

        // Confirm the linked appointment
        if (transaction.Appointment.Status == AppointmentStatus.PendingPayment)
        {
            transaction.Appointment.Status = AppointmentStatus.Confirmed;
            transaction.Appointment.UpdatedAt = _dateTimeProvider.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return TransactionDto.FromEntity(transaction);
    }
}
