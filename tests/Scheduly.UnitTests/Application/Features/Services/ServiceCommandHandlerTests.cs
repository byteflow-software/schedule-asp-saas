using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Services.Commands.CreateService;
using Scheduly.Application.Features.Services.Commands.UpdateService;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Services;

public class ServiceCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();

    public ServiceCommandHandlerTests()
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
            Id = _serviceId, TenantId = _tenantId, Name = "Check-up",
            DurationMinutes = 60, PriceInCents = 5000, IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateService_ValidCommand_CreatesWithTenantIdAndDefaults()
    {
        var handler = new CreateServiceCommandHandler(_context, _tenantService, new DateTimeProvider());
        var command = new CreateServiceCommand("Dental Cleaning", "Deep cleaning", 45, 8000);

        var result = await handler.Handle(command, CancellationToken.None);

        var service = await _context.Services.FirstAsync(s => s.Id == result.Id);
        service.TenantId.Should().Be(_tenantId);
        service.IsActive.Should().BeTrue();
        service.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task CreateService_ReturnsCorrectDto()
    {
        var handler = new CreateServiceCommandHandler(_context, _tenantService, new DateTimeProvider());
        var command = new CreateServiceCommand("X-Ray", "Full body X-Ray", 30, 12000);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("X-Ray");
        result.Description.Should().Be("Full body X-Ray");
        result.DurationMinutes.Should().Be(30);
        result.PriceInCents.Should().Be(12000);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateService_ValidData_UpdatesAllFields()
    {
        var handler = new UpdateServiceCommandHandler(_context, new DateTimeProvider());
        var command = new UpdateServiceCommand(
            _serviceId, "Updated Name", "Updated Description", 90, 10000, false);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.DurationMinutes.Should().Be(90);
        result.PriceInCents.Should().Be(10000);
        result.IsActive.Should().BeFalse();

        var service = await _context.Services.IgnoreQueryFilters().FirstAsync(s => s.Id == _serviceId);
        service.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateService_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new UpdateServiceCommandHandler(_context, new DateTimeProvider());
        var command = new UpdateServiceCommand(
            Guid.NewGuid(), "Name", null, 30, 5000, true);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
