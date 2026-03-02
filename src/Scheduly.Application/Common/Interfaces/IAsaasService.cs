using Scheduly.Application.Common.Models.Asaas;

namespace Scheduly.Application.Common.Interfaces;

public interface IAsaasService
{
    Task<AsaasCustomerResponse> CreateOrUpdateCustomerAsync(
        string apiKey, string name, string cpfCnpj, string email, string? phone,
        string externalReference, CancellationToken ct);

    Task<AsaasPaymentResponse> CreatePaymentWithSplitAsync(
        string apiKey, string asaasCustomerId, int amountInCents,
        string description, string externalReference, CancellationToken ct);

    Task<AsaasAccountResponse> ValidateApiKeyAsync(string apiKey, CancellationToken ct);

    Task<AsaasWebhookListResponse> ListWebhooksAsync(string apiKey, CancellationToken ct);
}
