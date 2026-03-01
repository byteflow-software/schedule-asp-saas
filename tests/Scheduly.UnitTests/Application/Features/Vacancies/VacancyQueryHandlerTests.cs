using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Vacancies.Queries.GetVacancies;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.UnitTests.Application.Features.Vacancies;

public class VacancyQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();
    private readonly Guid _bookedVacancyId = Guid.NewGuid();
    private readonly Guid _availableVacancyId = Guid.NewGuid();

    public VacancyQueryHandlerTests()
    {
        _tenantService = new CurrentTenantService();
        _tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, _tenantService);

        SeedData();
    }

    private void SeedData()
    {
        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Test Clinic", Slug = "test-clinic",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Users.Add(new User
        {
            Id = _userId, TenantId = _tenantId, FullName = "Dr. Alice",
            Email = "alice@test.com", PasswordHash = "hash", Role = UserRole.Staff,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Services.Add(new Service
        {
            Id = _serviceId, TenantId = _tenantId, Name = "Check-up",
            DurationMinutes = 60, PriceInCents = 5000, IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        _context.Vacancies.Add(new Vacancy
        {
            Id = _bookedVacancyId, TenantId = _tenantId, UserId = _userId,
            ServiceId = _serviceId,
            StartTime = new DateTime(2025, 6, 1, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            IsBooked = true, CreatedAt = DateTime.UtcNow
        });

        _context.Vacancies.Add(new Vacancy
        {
            Id = _availableVacancyId, TenantId = _tenantId, UserId = _userId,
            ServiceId = _serviceId,
            StartTime = new DateTime(2025, 6, 2, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 6, 2, 10, 0, 0, DateTimeKind.Utc),
            IsBooked = false, CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetVacancies_ReturnsAll()
    {
        var handler = new GetVacanciesQueryHandler(_context);
        var query = new GetVacanciesQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetVacancies_AvailableOnlyFilter()
    {
        var handler = new GetVacanciesQueryHandler(_context);
        var query = new GetVacanciesQuery(AvailableOnly: true);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(_availableVacancyId);
        result.First().IsBooked.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
