using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Common.Models;
using Scheduly.Application.Features.Transactions.DTOs;
using Scheduly.Domain.Enums;

namespace Scheduly.Application.Features.Transactions.Queries.GetTransactions;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PaginatedList<TransactionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions
            .Include(t => t.Customer)
            .AsQueryable();

        if (request.From.HasValue)
            query = query.Where(t => t.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(t => t.CreatedAt <= request.To.Value);
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<TransactionStatus>(request.Status, true, out var status))
            query = query.Where(t => t.Status == status);
        if (request.CustomerId.HasValue)
            query = query.Where(t => t.CustomerId == request.CustomerId.Value);

        var projected = query.OrderByDescending(t => t.CreatedAt)
            .Select(t => new TransactionDto(
                t.Id, t.AppointmentId, t.CustomerId,
                t.Customer.FullName, t.ReferenceNumber, t.AmountInCents,
                t.Status.ToString(), t.Type.ToString(),
                t.Description, t.PaidAt, t.PaymentMethod,
                t.InvoiceUrl, t.PixQrCodeUrl, t.CreatedAt));

        return await PaginatedList<TransactionDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
