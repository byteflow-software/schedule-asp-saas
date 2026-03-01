using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Tenants.Queries.GetTenant;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.UnitTests.Application.Features.Tenants;

public class GetTenantQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetTenantQueryHandlerTests()
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
            IsActive = true, AsaasApiKey = "aact_123456", AsaasWalletId = "wallet_abc",
            Email = "clinic@test.com", Phone = "11999999999",
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ExistingTenant_ReturnsTenantDto()
    {
        var handler = new GetTenantQueryHandler(_context, _tenantService);
        var query = new GetTenantQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Id.Should().Be(_tenantId);
        result.Name.Should().Be("Test Clinic");
        result.Slug.Should().Be("test-clinic");
        result.HasAsaasIntegration.Should().BeTrue();
        result.AsaasWalletId.Should().Be("wallet_abc");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonExistentTenant_ThrowsEntityNotFound()
    {
        // Point to a non-existent tenant
        var nonExistentTenantService = new CurrentTenantService();
        nonExistentTenantService.SetTenantId(Guid.NewGuid());

        var handler = new GetTenantQueryHandler(_context, nonExistentTenantService);
        var query = new GetTenantQuery();

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
