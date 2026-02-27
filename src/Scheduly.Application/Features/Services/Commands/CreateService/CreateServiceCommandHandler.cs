using MediatR;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Services.DTOs;
using Scheduly.Domain.Entities;

namespace Scheduly.Application.Features.Services.Commands.CreateService;

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ServiceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateServiceCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ServiceDto> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            TenantId = _currentTenantService.TenantId,
            Name = request.Name,
            Description = request.Description,
            DurationMinutes = request.DurationMinutes,
            PriceInCents = request.PriceInCents,
            IsActive = true,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceDto.FromEntity(service);
    }
}
