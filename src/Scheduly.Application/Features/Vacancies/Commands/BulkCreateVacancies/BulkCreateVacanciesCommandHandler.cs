using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Vacancies.DTOs;
using Scheduly.Domain.Entities;

namespace Scheduly.Application.Features.Vacancies.Commands.BulkCreateVacancies;

public class BulkCreateVacanciesCommandHandler : IRequestHandler<BulkCreateVacanciesCommand, List<VacancyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BulkCreateVacanciesCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _tenantService = tenantService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<List<VacancyDto>> Handle(BulkCreateVacanciesCommand request, CancellationToken cancellationToken)
    {
        var ids = new List<Guid>();

        foreach (var slot in request.Slots)
        {
            var vacancy = new Vacancy
            {
                Id = Guid.NewGuid(),
                TenantId = _tenantService.TenantId,
                UserId = request.UserId,
                ServiceId = request.ServiceId,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                IsBooked = false,
                CreatedAt = _dateTimeProvider.UtcNow
            };
            _context.Vacancies.Add(vacancy);
            ids.Add(vacancy.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var results = await _context.Vacancies
            .Include(v => v.User).Include(v => v.Service)
            .Where(v => ids.Contains(v.Id))
            .ToListAsync(cancellationToken);

        return results.Select(VacancyDto.FromEntity).ToList();
    }
}
