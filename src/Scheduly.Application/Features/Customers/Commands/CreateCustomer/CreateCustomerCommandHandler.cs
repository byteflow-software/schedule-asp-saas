using MediatR;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Customers.DTOs;
using Scheduly.Domain.Entities;

namespace Scheduly.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateCustomerCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = _currentTenantService.TenantId,
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            Phone = request.Phone,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return CustomerDto.FromEntity(customer);
    }
}
