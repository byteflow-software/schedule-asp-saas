using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Appointments.DTOs;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailService _emailService;
    private readonly IAsaasService _asaasService;
    private readonly IErrorLogService _errorLogService;
    private readonly ILogger<CreateAppointmentCommandHandler> _logger;

    public CreateAppointmentCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        IDateTimeProvider dateTimeProvider,
        IEmailService emailService,
        IAsaasService asaasService,
        IErrorLogService errorLogService,
        ILogger<CreateAppointmentCommandHandler> logger)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _dateTimeProvider = dateTimeProvider;
        _emailService = emailService;
        _asaasService = asaasService;
        _errorLogService = errorLogService;
        _logger = logger;
    }

    public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenantService.TenantId;
        var now = _dateTimeProvider.UtcNow;

        // Verify service exists
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken)
            ?? throw new EntityNotFoundException("Service", request.ServiceId);

        // Check for time overlap on the same staff member
        var hasOverlap = await _context.Appointments
            .AnyAsync(a =>
                a.UserId == request.UserId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.StartTime < request.EndTime &&
                a.EndTime > request.StartTime,
                cancellationToken);

        if (hasOverlap)
            throw new AppointmentOverlapException();

        // Verify customer exists
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken)
            ?? throw new EntityNotFoundException("Customer", request.CustomerId);

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = request.CustomerId,
            UserId = request.UserId,
            ServiceId = request.ServiceId,
            VacancyId = request.VacancyId,
            PriceInCents = service.PriceInCents,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Notes = request.Notes,
            Status = AppointmentStatus.PendingPayment,
            CreatedAt = now
        };

        _context.Appointments.Add(appointment);

        // Mark vacancy as booked if provided
        if (request.VacancyId.HasValue)
        {
            var vacancy = await _context.Vacancies
                .FirstOrDefaultAsync(v => v.Id == request.VacancyId.Value && !v.IsBooked, cancellationToken);
            if (vacancy != null)
            {
                vacancy.IsBooked = true;
                vacancy.AppointmentId = appointment.Id;
                vacancy.UpdatedAt = now;
            }
        }

        // Generate reference number
        var today = now.Date;
        var tomorrow = today.AddDays(1);
        var todayCount = await _context.Transactions
            .IgnoreQueryFilters()
            .CountAsync(t => t.TenantId == tenantId && t.CreatedAt >= today && t.CreatedAt < tomorrow, cancellationToken);
        var referenceNumber = $"TXN-{now:yyyyMMdd}-{(todayCount + 1):D3}";

        // Create transaction
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AppointmentId = appointment.Id,
            CustomerId = request.CustomerId,
            ReferenceNumber = referenceNumber,
            AmountInCents = service.PriceInCents,
            Status = TransactionStatus.Pending,
            Type = TransactionType.Charge,
            Description = $"Agendamento - {service.Name}",
            CreatedAt = now
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        // Asaas integration: create payment if tenant has Asaas configured
        await TryCreateAsaasPaymentAsync(tenantId, customer, transaction, cancellationToken);

        // Send charge email (fire and forget, don't block)
        _ = _emailService.SendChargeEmailAsync(
            customer.Email, customer.FullName,
            service.PriceInCents, referenceNumber,
            request.StartTime, service.Name, cancellationToken);

        // Reload with nav properties
        var result = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.User)
            .Include(a => a.Service)
            .FirstAsync(a => a.Id == appointment.Id, cancellationToken);

        var txn = await _context.Transactions
            .Include(t => t.Customer)
            .FirstAsync(t => t.Id == transaction.Id, cancellationToken);

        return AppointmentDto.FromEntity(result, txn);
    }

    private async Task TryCreateAsaasPaymentAsync(
        Guid tenantId, Customer customer, Transaction transaction, CancellationToken ct)
    {
        try
        {
            var tenant = await _context.Tenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

            if (tenant == null || string.IsNullOrEmpty(tenant.AsaasApiKey))
                return;

            // Create or update customer in Asaas
            var asaasCustomer = await _asaasService.CreateOrUpdateCustomerAsync(
                tenant.AsaasApiKey,
                customer.FullName,
                customer.CpfCnpj,
                customer.Email,
                customer.Phone,
                customer.Id.ToString(),
                ct);

            customer.AsaasCustomerId = asaasCustomer.Id;

            // Create payment with platform split (handled by service via settings)
            var asaasPayment = await _asaasService.CreatePaymentWithSplitAsync(
                tenant.AsaasApiKey,
                asaasCustomer.Id,
                transaction.AmountInCents,
                transaction.Description ?? "Agendamento",
                transaction.Id.ToString(),
                ct);

            transaction.AsaasPaymentId = asaasPayment.Id;
            transaction.InvoiceUrl = asaasPayment.InvoiceUrl;
            transaction.PixQrCodeUrl = asaasPayment.PixQrCodeUrl;

            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Asaas payment for transaction {TransactionId}", transaction.Id);

            await _errorLogService.LogAsync(new Domain.Entities.ErrorLog
            {
                Level = "Error",
                Message = $"Falha ao criar pagamento Asaas para transação {transaction.ReferenceNumber}: {ex.Message}",
                ExceptionType = ex.GetType().FullName,
                StackTrace = ex.StackTrace,
                Source = "CreateAppointmentCommandHandler.TryCreateAsaasPaymentAsync",
                RequestPath = "/api/appointments",
                RequestMethod = "POST",
                TenantId = tenantId,
                ExternalRequestUrl = ex.Data["AsaasRequestUrl"] as string,
                ExternalResponseBody = ex.Data["AsaasResponseBody"] as string,
                ExternalStatusCode = ex.Data["AsaasStatusCode"] as int?,
            });
        }
    }
}
