using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Common.Models.Asaas;
using Scheduly.Application.Features.Appointments.Commands.CreateAppointment;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.MultiTenancy;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Application.Features.Appointments;

public class CreateAppointmentCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CurrentTenantService _tenantService;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _serviceId = Guid.NewGuid();

    public CreateAppointmentCommandHandlerTests()
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
            Id = _userId, TenantId = _tenantId, FullName = "Dr. Test",
            Email = "dr@test.com", PasswordHash = "hash", Role = UserRole.Admin,
            IsActive = true, CreatedAt = DateTime.UtcNow
        });

        _context.Customers.Add(new Customer
        {
            Id = _customerId, TenantId = _tenantId, FullName = "John Doe",
            Email = "john@test.com", CpfCnpj = "12345678901", CreatedAt = DateTime.UtcNow
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
    public async Task Handle_ValidCommand_CreatesAppointment()
    {
        var handler = CreateHandler();
        var command = new CreateAppointmentCommand(
            _customerId, _userId, _serviceId, null,
            DateTime.UtcNow.AddDays(1).Date.AddHours(10),
            DateTime.UtcNow.AddDays(1).Date.AddHours(11),
            "Check-up");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be("PendingPayment");
        result.CustomerName.Should().Be("John Doe");
    }

    [Fact]
    public async Task Handle_OverlappingAppointment_ThrowsOverlapException()
    {
        var handler = CreateHandler();
        var startTime = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        var endTime = DateTime.UtcNow.AddDays(2).Date.AddHours(11);

        // Create first appointment
        await handler.Handle(new CreateAppointmentCommand(
            _customerId, _userId, _serviceId, null, startTime, endTime, null), CancellationToken.None);

        // Try overlapping
        var act = () => handler.Handle(new CreateAppointmentCommand(
            _customerId, _userId, _serviceId, null,
            startTime.AddMinutes(30), endTime.AddMinutes(30), null), CancellationToken.None);

        await act.Should().ThrowAsync<AppointmentOverlapException>();
    }

    private CreateAppointmentCommandHandler CreateHandler()
    {
        return new CreateAppointmentCommandHandler(
            _context, _tenantService, new DateTimeProvider(),
            new StubEmailService(), new StubAsaasService(),
            new StubErrorLogService(),
            NullLogger<CreateAppointmentCommandHandler>.Instance);
    }

    private class StubErrorLogService : IErrorLogService
    {
        public Task LogAsync(Scheduly.Domain.Entities.ErrorLog errorLog) => Task.CompletedTask;
    }

    private class StubEmailService : IEmailService
    {
        public Task SendChargeEmailAsync(string customerEmail, string customerName, int amountInCents, string referenceNumber, DateTime appointmentDate, string serviceName, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SendReminderAsync(string customerEmail, string customerName, DateTime appointmentTime, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private class StubAsaasService : IAsaasService
    {
        public Task<AsaasCustomerResponse> CreateOrUpdateCustomerAsync(string apiKey, string name, string cpfCnpj, string email, string? phone, string externalReference, CancellationToken ct)
            => Task.FromResult(new AsaasCustomerResponse("cus_test", name, cpfCnpj));

        public Task<AsaasPaymentResponse> CreatePaymentWithSplitAsync(string apiKey, string asaasCustomerId, int amountInCents, string description, string externalReference, CancellationToken ct)
            => Task.FromResult(new AsaasPaymentResponse("pay_test", "PENDING", "UNDEFINED", "https://invoice.test", null, null));

        public Task<AsaasAccountResponse> ValidateApiKeyAsync(string apiKey, CancellationToken ct)
            => Task.FromResult(new AsaasAccountResponse("wallet_test", "Test Account"));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
