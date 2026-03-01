using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Customers.Queries.GetCustomers;
using Scheduly.Application.Features.Customers.Queries.GetCustomerById;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;

namespace Scheduly.UnitTests.Application.Features.Customers;

public class CustomerQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _customer1Id = Guid.NewGuid();
    private readonly Guid _customer2Id = Guid.NewGuid();
    private readonly Guid _customer3Id = Guid.NewGuid();

    public CustomerQueryHandlerTests()
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

        _context.Customers.Add(new Customer
        {
            Id = _customer1Id, TenantId = _tenantId, FullName = "John Smith",
            Email = "john@test.com", Phone = "1111111111", CpfCnpj = "11111111111",
            CreatedAt = DateTime.UtcNow
        });

        _context.Customers.Add(new Customer
        {
            Id = _customer2Id, TenantId = _tenantId, FullName = "Jane Doe",
            Email = "jane@test.com", Phone = "2222222222", CpfCnpj = "22222222222",
            CreatedAt = DateTime.UtcNow
        });

        _context.Customers.Add(new Customer
        {
            Id = _customer3Id, TenantId = _tenantId, FullName = "Carlos Silva",
            Email = "carlos@test.com", Phone = "3333333333", CpfCnpj = "33333333333",
            CreatedAt = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetCustomers_ReturnsAll()
    {
        var handler = new GetCustomersQueryHandler(_context);
        var query = new GetCustomersQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetCustomers_SearchByName()
    {
        var handler = new GetCustomersQueryHandler(_context);
        var query = new GetCustomersQuery(Search: "John");

        var result = await handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.First().FullName.Should().Be("John Smith");
    }

    [Fact]
    public async Task GetCustomerById_ValidId_ReturnsDto()
    {
        var handler = new GetCustomerByIdQueryHandler(_context);
        var query = new GetCustomerByIdQuery(_customer1Id);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Id.Should().Be(_customer1Id);
        result.FullName.Should().Be("John Smith");
        result.Email.Should().Be("john@test.com");
        result.Phone.Should().Be("1111111111");
        result.CpfCnpj.Should().Be("11111111111");
    }

    [Fact]
    public async Task GetCustomerById_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new GetCustomerByIdQueryHandler(_context);
        var query = new GetCustomerByIdQuery(Guid.NewGuid());

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
