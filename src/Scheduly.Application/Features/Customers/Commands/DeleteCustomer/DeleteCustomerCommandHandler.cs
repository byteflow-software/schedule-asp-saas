using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Customers.Commands.DeleteCustomer;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteCustomerCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Customer", request.Id);

        customer.IsDeleted = true;
        customer.DeletedAt = _dateTimeProvider.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
