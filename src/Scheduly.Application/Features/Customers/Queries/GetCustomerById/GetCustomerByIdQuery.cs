using MediatR;
using Scheduly.Application.Features.Customers.DTOs;

namespace Scheduly.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto>;
