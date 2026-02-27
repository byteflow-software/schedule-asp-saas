using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Services.DTOs;

namespace Scheduly.Application.Features.Services.Queries.GetServices;

public class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, List<ServiceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceDto>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Services.AsQueryable();

        if (request.ActiveOnly == true)
        {
            query = query.Where(s => s.IsActive);
        }

        return await query
            .OrderBy(s => s.Name)
            .Select(s => new ServiceDto(s.Id, s.Name, s.Description, s.DurationMinutes, s.PriceInCents, s.IsActive, s.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
