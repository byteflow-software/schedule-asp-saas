using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Common.Models.Asaas;
using Scheduly.Application.Features.Tenants.Commands.ValidateAsaasKey;
using Scheduly.Application.Features.Tenants.DTOs;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Tenants;

public class ValidateAsaasKeyCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly DateTimeProvider _dateTimeProvider = new();
    private readonly StubAsaasService _asaasService = new();

    public ValidateAsaasKeyCommandHandlerTests()
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
            Id = _tenantId, Name = "Clinic", Slug = "clinic",
            IsActive = true, CreatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ValidKey_StoresKeyAndWalletId()
    {
        var handler = new ValidateAsaasKeyCommandHandler(
            _context, _tenantService, _asaasService, _dateTimeProvider);
        var command = new ValidateAsaasKeyCommand("test_api_key_123");

        await handler.Handle(command, CancellationToken.None);

        var tenant = await _context.Tenants.IgnoreQueryFilters()
            .FirstAsync(t => t.Id == _tenantId);

        tenant.AsaasApiKey.Should().Be("test_api_key_123");
        tenant.AsaasWalletId.Should().Be("wallet_test123");
        tenant.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ReturnsTenantDtoWithIntegration()
    {
        var handler = new ValidateAsaasKeyCommandHandler(
            _context, _tenantService, _asaasService, _dateTimeProvider);
        var command = new ValidateAsaasKeyCommand("test_api_key_123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeOfType<TenantDto>();
        result.HasAsaasIntegration.Should().BeTrue();
        result.AsaasWalletId.Should().Be("wallet_test123");
    }

    [Fact]
    public async Task Handle_NonExistentTenant_ThrowsEntityNotFound()
    {
        var otherTenantService = new CurrentTenantService();
        otherTenantService.SetTenantId(Guid.NewGuid());

        var handler = new ValidateAsaasKeyCommandHandler(
            _context, otherTenantService, _asaasService, _dateTimeProvider);
        var command = new ValidateAsaasKeyCommand("test_api_key_123");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private class StubAsaasService : IAsaasService
    {
        public Task<AsaasCustomerResponse> CreateOrUpdateCustomerAsync(
            string apiKey, string name, string cpfCnpj, string email, string? phone,
            string externalReference, CancellationToken ct)
            => Task.FromResult(new AsaasCustomerResponse("cus_test123", "Test Customer", "12345678901"));

        public Task<AsaasPaymentResponse> CreatePaymentWithSplitAsync(
            string apiKey, string asaasCustomerId, int amountInCents,
            string description, string externalReference, CancellationToken ct)
            => Task.FromResult(new AsaasPaymentResponse("pay_test123", "PENDING", null, null, null, null));

        public Task<AsaasAccountResponse> ValidateApiKeyAsync(string apiKey, CancellationToken ct)
            => Task.FromResult(new AsaasAccountResponse("wallet_test123", "Test Account"));
    }
}
