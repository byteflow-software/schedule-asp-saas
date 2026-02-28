using MediatR;
using Scheduly.Application.Features.Customers.DTOs;

namespace Scheduly.Application.Features.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    string CpfCnpj) : IRequest<CustomerDto>;
