using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Services.DTOs;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Services.Queries.GetServiceById;

public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceDto>
{
    private readonly IApplicationDbContext _context;

    public GetServiceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceDto> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Service", request.Id);

        return ServiceDto.FromEntity(service);
    }
}
