using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Services.DTOs;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Services.Commands.UpdateService;

public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ServiceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateServiceCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ServiceDto> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Service", request.Id);

        service.Name = request.Name;
        service.Description = request.Description;
        service.DurationMinutes = request.DurationMinutes;
        service.PriceInCents = request.PriceInCents;
        service.IsActive = request.IsActive;
        service.UpdatedAt = _dateTimeProvider.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ServiceDto.FromEntity(service);
    }
}
