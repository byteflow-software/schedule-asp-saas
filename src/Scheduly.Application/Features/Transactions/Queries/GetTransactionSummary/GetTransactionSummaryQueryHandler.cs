using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Transactions.DTOs;
using Scheduly.Domain.Enums;

namespace Scheduly.Application.Features.Transactions.Queries.GetTransactionSummary;

public class GetTransactionSummaryQueryHandler : IRequestHandler<GetTransactionSummaryQuery, TransactionSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionSummaryDto> Handle(GetTransactionSummaryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transactions.AsQueryable();

        if (request.From.HasValue)
            query = query.Where(t => t.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(t => t.CreatedAt <= request.To.Value);

        var transactions = await query.Select(t => new { t.AmountInCents, t.Status }).ToListAsync(cancellationToken);

        var totalRevenue = transactions.Where(t => t.Status != TransactionStatus.Cancelled).Sum(t => t.AmountInCents);
        var totalPending = transactions.Where(t => t.Status == TransactionStatus.Pending).Sum(t => t.AmountInCents);
        var totalPaid = transactions.Where(t => t.Status == TransactionStatus.Paid).Sum(t => t.AmountInCents);

        return new TransactionSummaryDto(totalRevenue, totalPending, totalPaid, transactions.Count);
    }
}
