using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Scheduly.IntegrationTests.Features.Auth;

public class AuthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            TenantName = "Test Clinic",
            FullName = "Admin User",
            Email = $"admin-{Guid.NewGuid()}@test.com",
            Password = "Password123!"
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, because: body);

        var content = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        content.Should().NotBeNull();
        content!.TenantId.Should().NotBeEmpty();
        content.UserId.Should().NotBeEmpty();
        content.Tokens.Should().NotBeNull();
        content.Tokens.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_AfterRegister_ReturnsOkWithTokens()
    {
        var email = $"login-{Guid.NewGuid()}@test.com";
        var password = "Password123!";

        // Register first
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            TenantName = "Login Test Clinic",
            FullName = "Admin User",
            Email = email,
            Password = password
        });

        // Login
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        content.Should().NotBeNull();
        content!.Tokens.AccessToken.Should().NotBeNullOrEmpty();
        content.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "nonexistent@test.com",
            Password = "wrong"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/tenants/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private record TokenResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);
    private record RegisterResponse(Guid TenantId, Guid UserId, TokenResponseDto Tokens);
    private record LoginResponse(Guid UserId, Guid TenantId, string FullName, string Role, TokenResponseDto Tokens);
}
