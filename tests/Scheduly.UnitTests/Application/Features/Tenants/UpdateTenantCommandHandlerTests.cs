using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Tenants.Commands.UpdateTenant;
using Scheduly.Application.Features.Tenants.DTOs;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Tenants;

public class UpdateTenantCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly DateTimeProvider _dateTimeProvider = new();

    public UpdateTenantCommandHandlerTests()
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
            Id = _tenantId, Name = "Old Clinic", Slug = "old-clinic",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ValidData_UpdatesAllFields()
    {
        var handler = new UpdateTenantCommandHandler(_context, _tenantService, _dateTimeProvider);
        var command = new UpdateTenantCommand(
            Name: "New Clinic",
            CpfCnpj: "12345678000199",
            Phone: "+5511999999999",
            Email: "clinic@example.com",
            Address: "Rua A",
            AddressNumber: "100",
            Complement: "Sala 1",
            Neighborhood: "Centro",
            City: "Sao Paulo",
            State: "SP",
            PostalCode: "01000-000",
            LogoUrl: "https://example.com/logo.png");

        await handler.Handle(command, CancellationToken.None);

        var tenant = await _context.Tenants.IgnoreQueryFilters()
            .FirstAsync(t => t.Id == _tenantId);

        tenant.Name.Should().Be("New Clinic");
        tenant.CpfCnpj.Should().Be("12345678000199");
        tenant.Phone.Should().Be("+5511999999999");
        tenant.Email.Should().Be("clinic@example.com");
        tenant.Address.Should().Be("Rua A");
        tenant.AddressNumber.Should().Be("100");
        tenant.Complement.Should().Be("Sala 1");
        tenant.Neighborhood.Should().Be("Centro");
        tenant.City.Should().Be("Sao Paulo");
        tenant.State.Should().Be("SP");
        tenant.PostalCode.Should().Be("01000-000");
        tenant.LogoUrl.Should().Be("https://example.com/logo.png");
        tenant.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ReturnsTenantDto()
    {
        var handler = new UpdateTenantCommandHandler(_context, _tenantService, _dateTimeProvider);
        var command = new UpdateTenantCommand(
            Name: "Updated Clinic", CpfCnpj: null, Phone: null, Email: null,
            Address: null, AddressNumber: null, Complement: null,
            Neighborhood: null, City: null, State: null, PostalCode: null, LogoUrl: null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<TenantDto>();
        result.Id.Should().Be(_tenantId);
        result.Name.Should().Be("Updated Clinic");
    }

    [Fact]
    public async Task Handle_NonExistentTenant_ThrowsEntityNotFound()
    {
        // Set tenant service to a non-existent tenant
        var otherTenantService = new CurrentTenantService();
        otherTenantService.SetTenantId(Guid.NewGuid());

        var handler = new UpdateTenantCommandHandler(_context, otherTenantService, _dateTimeProvider);
        var command = new UpdateTenantCommand(
            Name: "Name", CpfCnpj: null, Phone: null, Email: null,
            Address: null, AddressNumber: null, Complement: null,
            Neighborhood: null, City: null, State: null, PostalCode: null, LogoUrl: null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
