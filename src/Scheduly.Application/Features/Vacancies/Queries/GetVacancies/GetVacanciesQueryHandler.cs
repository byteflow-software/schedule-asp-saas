using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Vacancies.DTOs;

namespace Scheduly.Application.Features.Vacancies.Queries.GetVacancies;

public class GetVacanciesQueryHandler : IRequestHandler<GetVacanciesQuery, List<VacancyDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVacanciesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VacancyDto>> Handle(GetVacanciesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Vacancies
            .Include(v => v.User)
            .Include(v => v.Service)
            .AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(v => v.UserId == request.UserId.Value);
        if (request.ServiceId.HasValue)
            query = query.Where(v => v.ServiceId == request.ServiceId.Value);
        if (request.From.HasValue)
            query = query.Where(v => v.StartTime >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(v => v.StartTime <= request.To.Value);
        if (request.AvailableOnly == true)
            query = query.Where(v => !v.IsBooked);

        var vacancies = await query.OrderBy(v => v.StartTime).ToListAsync(cancellationToken);
        return vacancies.Select(VacancyDto.FromEntity).ToList();
    }
}
