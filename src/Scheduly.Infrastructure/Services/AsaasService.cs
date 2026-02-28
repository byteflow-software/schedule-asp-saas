using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Common.Models.Asaas;

namespace Scheduly.Infrastructure.Services;

public class AsaasService : IAsaasService
{
    private readonly HttpClient _httpClient;
    private readonly AsaasSettings _settings;
    private readonly ILogger<AsaasService> _logger;

    private const string ProductionBaseUrl = "https://api.asaas.com/v3/";
    private const string SandboxBaseUrl = "https://sandbox.asaas.com/api/v3/";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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
        }, options: JsonOptions);

        var response = await _httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Asaas CreateCustomer failed: {StatusCode} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Asaas API error: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<JsonElement>(body);
        return new AsaasCustomerResponse(
            result.GetProperty("id").GetString()!,
            result.GetProperty("name").GetString()!,
            result.GetProperty("cpfCnpj").GetString()!);
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

        // Add platform split if configured
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

        request.Content = JsonContent.Create(payload, options: JsonOptions);

        var response = await _httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Asaas CreatePayment failed: {StatusCode} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Asaas API error: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<JsonElement>(body);
        return new AsaasPaymentResponse(
            result.GetProperty("id").GetString()!,
            result.GetProperty("status").GetString()!,
            result.TryGetProperty("billingType", out var bt) ? bt.GetString() : null,
            result.TryGetProperty("invoiceUrl", out var iu) ? iu.GetString() : null,
            result.TryGetProperty("bankSlipUrl", out var bs) ? bs.GetString() : null,
            result.TryGetProperty("pixQrCodeUrl", out var pq) ? pq.GetString() : null);
    }

    public async Task<AsaasAccountResponse> ValidateApiKeyAsync(string apiKey, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, GetBaseUrl(apiKey) + "myAccount");
        request.Headers.Add("access_token", apiKey);

        var response = await _httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Asaas ValidateApiKey failed: {StatusCode} {Body}", response.StatusCode, body);
            throw new InvalidOperationException("Invalid Asaas API key.");
        }

        var result = JsonSerializer.Deserialize<JsonElement>(body);
        return new AsaasAccountResponse(
            result.GetProperty("walletId").GetString()!,
            result.GetProperty("name").GetString()!);
    }
}
