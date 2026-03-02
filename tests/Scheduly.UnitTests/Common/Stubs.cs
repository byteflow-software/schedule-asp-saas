using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Common.Models.Asaas;

namespace Scheduly.UnitTests.Common;

public class StubEmailService : IEmailService
{
    public Task SendChargeEmailAsync(
        string customerEmail,
        string customerName,
        int amountInCents,
        string referenceNumber,
        DateTime appointmentDate,
        string serviceName,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task SendReminderAsync(
        string customerEmail,
        string customerName,
        DateTime appointmentTime,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public class StubAsaasService : IAsaasService
{
    public Task<AsaasCustomerResponse> CreateOrUpdateCustomerAsync(
        string apiKey, string name, string cpfCnpj, string email, string? phone,
        string externalReference, CancellationToken ct)
        => Task.FromResult(new AsaasCustomerResponse("cus_test", name, cpfCnpj));

    public Task<AsaasPaymentResponse> CreatePaymentWithSplitAsync(
        string apiKey, string asaasCustomerId, int amountInCents,
        string description, string externalReference, CancellationToken ct)
        => Task.FromResult(new AsaasPaymentResponse("pay_test", "PENDING", "UNDEFINED", "https://invoice.test", null, "https://pix.test"));

    public Task<AsaasAccountResponse> ValidateApiKeyAsync(string apiKey, CancellationToken ct)
        => Task.FromResult(new AsaasAccountResponse("wallet_test123", "Test Account"));

    public Task<AsaasWebhookListResponse> ListWebhooksAsync(string apiKey, CancellationToken ct)
        => Task.FromResult(new AsaasWebhookListResponse([]));
}
