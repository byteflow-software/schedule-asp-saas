using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Customers.DTOs;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Customer", request.Id);

        return CustomerDto.FromEntity(customer);
    }
}
