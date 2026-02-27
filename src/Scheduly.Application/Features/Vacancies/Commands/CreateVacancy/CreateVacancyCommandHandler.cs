using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Vacancies.DTOs;
using Scheduly.Domain.Entities;

namespace Scheduly.Application.Features.Vacancies.Commands.CreateVacancy;

public class CreateVacancyCommandHandler : IRequestHandler<CreateVacancyCommand, VacancyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateVacancyCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _tenantService = tenantService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<VacancyDto> Handle(CreateVacancyCommand request, CancellationToken cancellationToken)
    {
        var vacancy = new Vacancy
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantService.TenantId,
            UserId = request.UserId,
            ServiceId = request.ServiceId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsBooked = false,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        _context.Vacancies.Add(vacancy);
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.Vacancies
            .Include(v => v.User).Include(v => v.Service)
            .FirstAsync(v => v.Id == vacancy.Id, cancellationToken);

        return VacancyDto.FromEntity(result);
    }
}
