using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Transactions.DTOs;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Transactions.Queries.GetTransactionById;

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionDto> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var t = await _context.Transactions
            .Include(t => t.Customer)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Transaction", request.Id);

        return TransactionDto.FromEntity(t);
    }
}
