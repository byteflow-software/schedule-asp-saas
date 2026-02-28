using MediatR;
using Scheduly.Application.Features.Customers.DTOs;

namespace Scheduly.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string FullName,
    string Email,
    string? Phone,
    string CpfCnpj) : IRequest<CustomerDto>;
