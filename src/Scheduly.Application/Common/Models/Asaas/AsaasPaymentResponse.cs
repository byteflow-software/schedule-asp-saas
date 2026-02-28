namespace Scheduly.Application.Common.Models.Asaas;

public record AsaasPaymentResponse(
    string Id,
    string Status,
    string? BillingType,
    string? InvoiceUrl,
    string? BankSlipUrl,
    string? PixQrCodeUrl);
