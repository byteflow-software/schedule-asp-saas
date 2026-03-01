using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Scheduly.IntegrationTests.Helpers;

public static class AuthHelper
{
    private record TokenResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);
    private record RegisterResponse(Guid TenantId, Guid UserId, TokenResponseDto Tokens);

    /// <summary>
    /// Registers a new tenant + admin user and configures the HttpClient with
    /// the Bearer token and X-Tenant-Id header. Returns the token and tenantId.
    /// </summary>
    public static async Task<(string Token, string TenantId)> RegisterAndAuthAsync(HttpClient client)
    {
        var registerBody = new
        {
            TenantName = "Test Tenant",
            FullName = "Admin User",
            Email = $"admin-{Guid.NewGuid()}@test.com",
            Password = "Password123!"
        };

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerBody);
        registerResponse.EnsureSuccessStatusCode();

        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        var token = registered!.Tokens.AccessToken;
        var tenantId = registered.TenantId.ToString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);

        return (token, tenantId);
    }

    /// <summary>
    /// Creates a service via the API and returns the created service id.
    /// Requires an already-authenticated client.
    /// </summary>
    public static async Task<Guid> CreateServiceAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/services", new
        {
            Name = $"Service {Guid.NewGuid():N}",
            Description = "Integration test service",
            DurationMinutes = 60,
            PriceInCents = 5000
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return Guid.Parse(result.GetProperty("id").GetString()!);
    }

    /// <summary>
    /// Creates a customer via the API and returns the created customer id.
    /// Requires an already-authenticated client.
    /// </summary>
    public static async Task<Guid> CreateCustomerAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/customers", new
        {
            FullName = $"Customer {Guid.NewGuid():N}",
            Email = $"customer-{Guid.NewGuid()}@test.com",
            Phone = "+5511999990000",
            CpfCnpj = "12345678901"
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return Guid.Parse(result.GetProperty("id").GetString()!);
    }

    /// <summary>
    /// Creates a user (Staff role) via the API and returns the created user id.
    /// Requires an already-authenticated client with Admin role.
    /// </summary>
    public static async Task<Guid> CreateUserAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/users", new
        {
            FullName = $"Staff {Guid.NewGuid():N}",
            Email = $"staff-{Guid.NewGuid()}@test.com",
            Password = "Password123!",
            Role = "Staff"
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return Guid.Parse(result.GetProperty("id").GetString()!);
    }
}
