using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Vacancies.Queries.GetVacancies;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.UnitTests.Application.Features.Vacancies;

public class VacancyQueryFilterTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _user1Id = Guid.NewGuid();
    private readonly Guid _user2Id = Guid.NewGuid();
    private readonly Guid _service1Id = Guid.NewGuid();
    private readonly Guid _service2Id = Guid.NewGuid();

    public VacancyQueryFilterTests()
    {
        var tenantService = new CurrentTenantService();
        tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, tenantService);
        SeedData();
    }

    private void SeedData()
    {
        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Test", Slug = "test",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Users.Add(new User
        {
            Id = _user1Id, TenantId = _tenantId, FullName = "Dr. One",
            Email = "one@test.com", PasswordHash = "h", Role = UserRole.Staff,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.Users.Add(new User
        {
            Id = _user2Id, TenantId = _tenantId, FullName = "Dr. Two",
            Email = "two@test.com", PasswordHash = "h", Role = UserRole.Staff,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Services.Add(new Service
        {
            Id = _service1Id, TenantId = _tenantId, Name = "Svc1",
            DurationMinutes = 30, PriceInCents = 1000, IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.Services.Add(new Service
        {
            Id = _service2Id, TenantId = _tenantId, Name = "Svc2",
            DurationMinutes = 60, PriceInCents = 2000, IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Vacancies.Add(new Vacancy
        {
            Id = Guid.NewGuid(), TenantId = _tenantId, UserId = _user1Id, ServiceId = _service1Id,
            StartTime = new DateTime(2025, 7, 1, 9, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 7, 1, 9, 30, 0, DateTimeKind.Utc),
            IsBooked = false, CreatedAt = DateTime.UtcNow
        });
        _context.Vacancies.Add(new Vacancy
        {
            Id = Guid.NewGuid(), TenantId = _tenantId, UserId = _user2Id, ServiceId = _service2Id,
            StartTime = new DateTime(2025, 7, 2, 10, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2025, 7, 2, 11, 0, 0, DateTimeKind.Utc),
            IsBooked = true, CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task FilterByUserId_ReturnsOnlyMatchingUser()
    {
        var handler = new GetVacanciesQueryHandler(_context);
        var result = await handler.Handle(
            new GetVacanciesQuery(UserId: _user1Id), CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().UserName.Should().Be("Dr. One");
    }

    [Fact]
    public async Task FilterByServiceId_ReturnsOnlyMatchingService()
    {
        var handler = new GetVacanciesQueryHandler(_context);
        var result = await handler.Handle(
            new GetVacanciesQuery(ServiceId: _service2Id), CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().ServiceName.Should().Be("Svc2");
    }

    [Fact]
    public async Task FilterByFrom_ReturnsOnlyAfterDate()
    {
        var handler = new GetVacanciesQueryHandler(_context);
        var result = await handler.Handle(
            new GetVacanciesQuery(From: new DateTime(2025, 7, 2, 0, 0, 0, DateTimeKind.Utc)),
            CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task FilterByTo_ReturnsOnlyBeforeDate()
    {
        var handler = new GetVacanciesQueryHandler(_context);
        var result = await handler.Handle(
            new GetVacanciesQuery(To: new DateTime(2025, 7, 1, 23, 59, 59, DateTimeKind.Utc)),
            CancellationToken.None);

        result.Should().HaveCount(1);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
