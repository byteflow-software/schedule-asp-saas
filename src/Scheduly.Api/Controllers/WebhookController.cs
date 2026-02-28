using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scheduly.Domain.Enums;
using Scheduly.Infrastructure.Persistence;
using Scheduly.Infrastructure.Services;

namespace Scheduly.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AsaasSettings _asaasSettings;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        ApplicationDbContext context,
        IOptions<AsaasSettings> asaasSettings,
        ILogger<WebhookController> logger)
    {
        _context = context;
        _asaasSettings = asaasSettings.Value;
        _logger = logger;
    }

    [HttpPost("asaas")]
    public async Task<IActionResult> AsaasWebhook([FromBody] JsonElement payload)
    {
        // Validate webhook token
        var token = Request.Headers["asaas-access-token"].FirstOrDefault()
            ?? Request.Query["token"].FirstOrDefault();

        if (!string.IsNullOrEmpty(_asaasSettings.WebhookToken) && token != _asaasSettings.WebhookToken)
        {
            _logger.LogWarning("Invalid Asaas webhook token received");
            return Unauthorized();
        }

        var eventType = payload.TryGetProperty("event", out var evt) ? evt.GetString() : null;
        if (string.IsNullOrEmpty(eventType))
            return BadRequest("Missing event type");

        var payment = payload.TryGetProperty("payment", out var p) ? p : default;
        if (payment.ValueKind == JsonValueKind.Undefined)
            return BadRequest("Missing payment data");

        var asaasPaymentId = payment.TryGetProperty("id", out var id) ? id.GetString() : null;
        var externalReference = payment.TryGetProperty("externalReference", out var er) ? er.GetString() : null;

        _logger.LogInformation("Asaas webhook received: {Event} for payment {PaymentId}, ref: {Reference}",
            eventType, asaasPaymentId, externalReference);

        // Find transaction by AsaasPaymentId or externalReference (transaction ID)
        var transaction = await FindTransactionAsync(asaasPaymentId, externalReference);
        if (transaction == null)
        {
            _logger.LogWarning("Transaction not found for Asaas payment {PaymentId} / ref {Reference}",
                asaasPaymentId, externalReference);
            return Ok(); // Return OK to avoid retries
        }

        var now = DateTime.UtcNow;

        switch (eventType)
        {
            case "PAYMENT_CONFIRMED":
            case "PAYMENT_RECEIVED":
                if (transaction.Status == TransactionStatus.Paid)
                    return Ok(); // Idempotent

                transaction.Status = TransactionStatus.Paid;
                transaction.PaidAt = now;
                transaction.PaymentMethod = payment.TryGetProperty("billingType", out var bt) ? bt.GetString() : null;
                transaction.UpdatedAt = now;

                // Confirm linked appointment
                var appointment = await _context.Appointments
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(a => a.Id == transaction.AppointmentId);
                if (appointment != null && appointment.Status == AppointmentStatus.PendingPayment)
                {
                    appointment.Status = AppointmentStatus.Confirmed;
                    appointment.UpdatedAt = now;
                }
                break;

            case "PAYMENT_REFUNDED":
                if (transaction.Status == TransactionStatus.Refunded)
                    return Ok();

                transaction.Status = TransactionStatus.Refunded;
                transaction.UpdatedAt = now;
                break;

            default:
                _logger.LogInformation("Unhandled Asaas event: {Event}", eventType);
                return Ok();
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    private async Task<Domain.Entities.Transaction?> FindTransactionAsync(string? asaasPaymentId, string? externalReference)
    {
        if (!string.IsNullOrEmpty(asaasPaymentId))
        {
            var byPaymentId = await _context.Transactions
                .IgnoreQueryFilters()
                .Include(t => t.Appointment)
                .FirstOrDefaultAsync(t => t.AsaasPaymentId == asaasPaymentId);
            if (byPaymentId != null) return byPaymentId;
        }

        if (!string.IsNullOrEmpty(externalReference) && Guid.TryParse(externalReference, out var txnId))
        {
            return await _context.Transactions
                .IgnoreQueryFilters()
                .Include(t => t.Appointment)
                .FirstOrDefaultAsync(t => t.Id == txnId);
        }

        return null;
    }
}
