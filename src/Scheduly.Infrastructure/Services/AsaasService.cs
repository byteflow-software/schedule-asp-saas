using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Common.Models.Asaas;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Infrastructure.Services;

public class AsaasService : IAsaasService
{
    private readonly HttpClient _httpClient;
    private readonly AsaasSettings _settings;
    private readonly ILogger<AsaasService> _logger;

    private const string ProductionBaseUrl = "https://api.asaas.com/v3/";
    private const string SandboxBaseUrl = "https://sandbox.asaas.com/api/v3/";

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AsaasService(HttpClient httpClient, IOptions<AsaasSettings> settings, ILogger<AsaasService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    private static string GetBaseUrl(string apiKey)
    {
        return apiKey.Contains("_hmlg_", StringComparison.OrdinalIgnoreCase)
            ? SandboxBaseUrl
            : ProductionBaseUrl;
    }

    public async Task<AsaasCustomerResponse> CreateOrUpdateCustomerAsync(
        string apiKey, string name, string cpfCnpj, string email, string? phone,
        string externalReference, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, GetBaseUrl(apiKey) + "customers");
        request.Headers.Add("access_token", apiKey);
        request.Content = JsonContent.Create(new
        {
            name,
            cpfCnpj,
            email,
            phone,
            externalReference
        }, options: WriteOptions);

        var response = await _httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Asaas CreateCustomer failed: {StatusCode} {Body}", response.StatusCode, body);
            throw AsaasException("ASAAS_ERROR",
                $"Erro na API do Asaas ao criar cliente: {response.StatusCode}",
                request.RequestUri, body, (int)response.StatusCode);
        }

        _logger.LogDebug("Asaas CreateCustomer response: {Body}", body);
        var result = JsonSerializer.Deserialize<AsaasApiCustomerResult>(body, ReadOptions)
            ?? throw new DomainException("ASAAS_PARSE_ERROR", "Resposta inesperada da API do Asaas.");

        return new AsaasCustomerResponse(
            result.Id ?? throw new DomainException("ASAAS_PARSE_ERROR", "Resposta da API do Asaas sem ID do cliente."),
            result.Name ?? "",
            result.CpfCnpj ?? "");
    }

    public async Task<AsaasPaymentResponse> CreatePaymentWithSplitAsync(
        string apiKey, string asaasCustomerId, int amountInCents,
        string description, string externalReference, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, GetBaseUrl(apiKey) + "payments");
        request.Headers.Add("access_token", apiKey);

        var payload = new Dictionary<string, object>
        {
            ["customer"] = asaasCustomerId,
            ["billingType"] = "UNDEFINED",
            ["value"] = amountInCents / 100.0m,
            ["dueDate"] = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-dd"),
            ["description"] = description,
            ["externalReference"] = externalReference
        };

        if (!string.IsNullOrEmpty(_settings.PlatformWalletId) && _settings.PlatformSplitPercent > 0)
        {
            payload["split"] = new[]
            {
                new
                {
                    walletId = _settings.PlatformWalletId,
                    percentualValue = _settings.PlatformSplitPercent,
                    totalFixedValue = (decimal?)null
                }
            };
        }

        request.Content = JsonContent.Create(payload, options: WriteOptions);

        var response = await _httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Asaas CreatePayment failed: {StatusCode} {Body}", response.StatusCode, body);
            throw AsaasException("ASAAS_ERROR",
                $"Erro na API do Asaas ao criar pagamento: {response.StatusCode}",
                request.RequestUri, body, (int)response.StatusCode);
        }

        _logger.LogDebug("Asaas CreatePayment response: {Body}", body);
        var result = JsonSerializer.Deserialize<AsaasApiPaymentResult>(body, ReadOptions)
            ?? throw new DomainException("ASAAS_PARSE_ERROR", "Resposta inesperada da API do Asaas.");

        return new AsaasPaymentResponse(
            result.Id ?? throw new DomainException("ASAAS_PARSE_ERROR", "Resposta da API do Asaas sem ID do pagamento."),
            result.Status ?? "PENDING",
            result.BillingType,
            result.InvoiceUrl,
            result.BankSlipUrl,
            result.PixQrCodeUrl);
    }

    public async Task<AsaasAccountResponse> ValidateApiKeyAsync(string apiKey, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, GetBaseUrl(apiKey) + "myAccount");
        request.Headers.Add("access_token", apiKey);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to Asaas API");
            throw new DomainException("ASAAS_CONNECTION_ERROR", "Não foi possível conectar à API do Asaas. Tente novamente.");
        }

        var body = await response.Content.ReadAsStringAsync(ct);

        _logger.LogInformation("Asaas ValidateApiKey status: {StatusCode}, body: {Body}", response.StatusCode, body);

        if (!response.IsSuccessStatusCode)
        {
            throw AsaasException("INVALID_ASAAS_KEY",
                "Chave da API do Asaas inválida. Verifique e tente novamente.",
                request.RequestUri, body, (int)response.StatusCode);
        }

        var result = JsonSerializer.Deserialize<AsaasApiAccountResult>(body, ReadOptions)
            ?? throw new DomainException("ASAAS_PARSE_ERROR", "Resposta inesperada da API do Asaas.");

        return new AsaasAccountResponse(
            result.WalletId ?? result.Id ?? "unknown",
            result.Name ?? result.CompanyName ?? "");
    }

    private static DomainException AsaasException(string code, string message, Uri? requestUrl, string? responseBody, int statusCode)
    {
        var ex = new DomainException(code, message);
        ex.Data["AsaasRequestUrl"] = requestUrl?.ToString();
        ex.Data["AsaasResponseBody"] = responseBody?.Length > 4000 ? responseBody[..4000] : responseBody;
        ex.Data["AsaasStatusCode"] = statusCode;
        return ex;
    }

    // Internal DTOs for Asaas API JSON responses
    private sealed class AsaasApiCustomerResult
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? CpfCnpj { get; set; }
    }

    private sealed class AsaasApiPaymentResult
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public string? BillingType { get; set; }
        public string? InvoiceUrl { get; set; }
        public string? BankSlipUrl { get; set; }
        public string? PixQrCodeUrl { get; set; }
    }

    private sealed class AsaasApiAccountResult
    {
        public string? Id { get; set; }
        public string? WalletId { get; set; }
        public string? Name { get; set; }
        public string? CompanyName { get; set; }
        public string? TradingName { get; set; }
    }
}
