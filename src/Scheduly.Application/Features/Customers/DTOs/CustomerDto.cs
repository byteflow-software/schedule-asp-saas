using Scheduly.Domain.Entities;

namespace Scheduly.Application.Features.Customers.DTOs;

public record CustomerDto(
    Guid Id,
    string FullName,
    string Email,
    string? Phone,
    DateTime CreatedAt)
{
    public static CustomerDto FromEntity(Customer customer) => new(
        customer.Id,
        customer.FullName,
        customer.Email,
        customer.Phone,
        customer.CreatedAt);
}
