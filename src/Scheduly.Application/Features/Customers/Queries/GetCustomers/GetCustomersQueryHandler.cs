using MediatR;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Common.Models;
using Scheduly.Application.Features.Customers.DTOs;

namespace Scheduly.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PaginatedList<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLowerInvariant();
            query = query.Where(c =>
                c.FullName.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search) ||
                (c.Phone != null && c.Phone.Contains(search)));
        }

        var projected = query
            .OrderBy(c => c.FullName)
            .Select(c => new CustomerDto(c.Id, c.FullName, c.Email, c.Phone, c.CpfCnpj, c.CreatedAt));

        return await PaginatedList<CustomerDto>.CreateAsync(
            projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
