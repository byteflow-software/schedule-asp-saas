using MediatR;
using Scheduly.Application.Common.Models;
using Scheduly.Application.Features.Customers.DTOs;

namespace Scheduly.Application.Features.Customers.Queries.GetCustomers;

public record GetCustomersQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<CustomerDto>>;
