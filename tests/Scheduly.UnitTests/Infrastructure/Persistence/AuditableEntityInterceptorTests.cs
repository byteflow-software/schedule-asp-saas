using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Persistence.Interceptors;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Infrastructure.Persistence;

public class AuditableEntityInterceptorTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public AuditableEntityInterceptorTests()
    {
        var tenantService = new CurrentTenantService();
        tenantService.SetTenantId(_tenantId);

        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(_userId);

        var interceptor = new AuditableEntityInterceptor(currentUserService, new DateTimeProvider());

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        _context = new ApplicationDbContext(options, tenantService);

        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Test", Slug = "test",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task SavingChanges_AddedEntity_SetsCreatedAtAndCreatedBy()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(), TenantId = _tenantId,
            FullName = "New Customer", Email = "new@test.com",
            CpfCnpj = "12345678901"
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        customer.CreatedBy.Should().Be(_userId.ToString());
    }

    [Fact]
    public async Task SavingChanges_ModifiedEntity_SetsUpdatedAt()
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(), TenantId = _tenantId,
            FullName = "Existing", Email = "existing@test.com",
            CpfCnpj = "12345678901", CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        customer.FullName = "Updated";
        _context.Entry(customer).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
