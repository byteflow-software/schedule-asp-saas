using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Services.Queries.GetServices;
using Scheduly.Application.Features.Services.Queries.GetServiceById;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.UnitTests.Application.Features.Services;

public class ServiceQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _activeServiceId = Guid.NewGuid();
    private readonly Guid _inactiveServiceId = Guid.NewGuid();

    public ServiceQueryHandlerTests()
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

        _context.Services.Add(new Service
        {
            Id = _activeServiceId, TenantId = _tenantId, Name = "Check-up",
            Description = "Regular check-up", DurationMinutes = 60, PriceInCents = 5000,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Services.Add(new Service
        {
            Id = _inactiveServiceId, TenantId = _tenantId, Name = "Old Service",
            Description = "Discontinued", DurationMinutes = 30, PriceInCents = 3000,
            IsActive = false, CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetServices_ReturnsAll()
    {
        var handler = new GetServicesQueryHandler(_context);
        var query = new GetServicesQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetServices_ActiveOnlyFilter()
    {
        var handler = new GetServicesQueryHandler(_context);
        var query = new GetServicesQuery(ActiveOnly: true);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Check-up");
        result.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetServiceById_ValidId_ReturnsDto()
    {
        var handler = new GetServiceByIdQueryHandler(_context);
        var query = new GetServiceByIdQuery(_activeServiceId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Id.Should().Be(_activeServiceId);
        result.Name.Should().Be("Check-up");
        result.Description.Should().Be("Regular check-up");
        result.DurationMinutes.Should().Be(60);
        result.PriceInCents.Should().Be(5000);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetServiceById_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new GetServiceByIdQueryHandler(_context);
        var query = new GetServiceByIdQuery(Guid.NewGuid());

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
