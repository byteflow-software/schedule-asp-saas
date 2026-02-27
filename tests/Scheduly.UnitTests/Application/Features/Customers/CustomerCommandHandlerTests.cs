using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Features.Customers.Commands.CreateCustomer;
using Scheduly.Application.Features.Customers.Commands.DeleteCustomer;
using Scheduly.Application.Features.Customers.Commands.UpdateCustomer;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Customers;

public class CustomerCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly DateTimeProvider _dateTimeProvider = new();

    public CustomerCommandHandlerTests()
    {
        _tenantService = new CurrentTenantService();
        _tenantService.SetTenantId(_tenantId);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, _tenantService);

        _context.Tenants.Add(new Tenant
        {
            Id = _tenantId, Name = "Clinic", Slug = "clinic",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    // ── CreateCustomer ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateCustomer_ValidCommand_ReturnsDto()
    {
        var handler = new CreateCustomerCommandHandler(_context, _tenantService, _dateTimeProvider);
        var command = new CreateCustomerCommand("Jane Doe", "jane@example.com", "+5511999999999");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.FullName.Should().Be("Jane Doe");
        result.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task CreateCustomer_EmailIsLowercased()
    {
        var handler = new CreateCustomerCommandHandler(_context, _tenantService, _dateTimeProvider);
        var command = new CreateCustomerCommand("Jane", "JANE@EXAMPLE.COM", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public async Task CreateCustomer_AssignsTenantId()
    {
        var handler = new CreateCustomerCommandHandler(_context, _tenantService, _dateTimeProvider);
        await handler.Handle(new CreateCustomerCommand("Jane", "j@ex.com", null), CancellationToken.None);

        var saved = await _context.Customers.IgnoreQueryFilters().FirstAsync();
        saved.TenantId.Should().Be(_tenantId);
        saved.IsDeleted.Should().BeFalse();
    }

    // ── UpdateCustomer ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateCustomer_ExistingCustomer_UpdatesFields()
    {
        var createHandler = new CreateCustomerCommandHandler(_context, _tenantService, _dateTimeProvider);
        var created = await createHandler.Handle(
            new CreateCustomerCommand("Old Name", "old@email.com", null), CancellationToken.None);

        var updateHandler = new UpdateCustomerCommandHandler(_context, _dateTimeProvider);
        var updateCmd = new UpdateCustomerCommand(created.Id, "New Name", "new@email.com", "+1");

        var result = await updateHandler.Handle(updateCmd, CancellationToken.None);

        result.FullName.Should().Be("New Name");
        result.Email.Should().Be("new@email.com");
        result.Phone.Should().Be("+1");
    }

    [Fact]
    public async Task UpdateCustomer_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new UpdateCustomerCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(
            new UpdateCustomerCommand(Guid.NewGuid(), "Name", "e@e.com", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── DeleteCustomer (soft delete) ─────────────────────────────────────────

    [Fact]
    public async Task DeleteCustomer_ExistingCustomer_SoftDeletesIt()
    {
        var createHandler = new CreateCustomerCommandHandler(_context, _tenantService, _dateTimeProvider);
        var created = await createHandler.Handle(
            new CreateCustomerCommand("To Delete", "del@email.com", null), CancellationToken.None);

        var deleteHandler = new DeleteCustomerCommandHandler(_context, _dateTimeProvider);
        await deleteHandler.Handle(new DeleteCustomerCommand(created.Id), CancellationToken.None);

        // Soft-deleted: IsDeleted = true, record still in DB
        var inDb = await _context.Customers.IgnoreQueryFilters()
            .FirstAsync(c => c.Id == created.Id);
        inDb.IsDeleted.Should().BeTrue();
        inDb.DeletedAt.Should().NotBeNull();

        // Query filter hides soft-deleted records
        var filtered = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == created.Id);
        filtered.Should().BeNull(because: "soft-deleted customers are hidden by query filter");
    }

    [Fact]
    public async Task DeleteCustomer_NonExistentId_ThrowsEntityNotFound()
    {
        var handler = new DeleteCustomerCommandHandler(_context, _dateTimeProvider);

        var act = () => handler.Handle(new DeleteCustomerCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
