using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Scheduly.IntegrationTests.Features.Auth;

/// <summary>
/// Extended integration tests for auth flows: refresh token, revoke token,
/// duplicate email rejection, and validation error shapes.
/// </summary>
public class AuthFlowIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthFlowIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Registration validations ─────────────────────────────────────────────

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns400()
    {
        var email = $"dup-{Guid.NewGuid()}@test.com";

        // First registration succeeds
        var first = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            TenantName = "Clinic A",
            FullName = "Admin A",
            Email = email,
            Password = "Password123!"
        });
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        // Second registration with same email must be rejected
        var second = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            TenantName = "Clinic B",
            FullName = "Admin B",
            Email = email,
            Password = "Password123!"
        });
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: "Duplicate emails must be rejected globally (critical bug fix for IgnoreQueryFilters in validator)");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            TenantName = "Clinic",
            FullName = "Admin",
            Email = "not-a-valid-email",
            Password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithShortPassword_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            TenantName = "Clinic",
            FullName = "Admin",
            Email = $"{Guid.NewGuid()}@test.com",
            Password = "short"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithEmptyTenantName_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            TenantName = "",
            FullName = "Admin",
            Email = $"{Guid.NewGuid()}@test.com",
            Password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Refresh token flow ───────────────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        var email = $"refresh-{Guid.NewGuid()}@test.com";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            TenantName = "Refresh Test Clinic",
            FullName = "Admin",
            Email = email,
            Password = "Password123!"
        });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        registered.Should().NotBeNull();

        // Use the refresh token from registration
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            Token = registered!.Tokens.RefreshToken
        });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokens = await refreshResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        newTokens.Should().NotBeNull();
        newTokens!.AccessToken.Should().NotBeNullOrEmpty();
        newTokens.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            Token = "this-is-not-a-valid-refresh-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_AfterRevoke_Returns400()
    {
        var email = $"revoke-{Guid.NewGuid()}@test.com";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            TenantName = "Revoke Test Clinic",
            FullName = "Admin",
            Email = email,
            Password = "Password123!"
        });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();

        // Revoke the token first
        var revokeResponse = await _client.PostAsJsonAsync("/api/auth/revoke-token", new
        {
            Token = registered!.Tokens.RefreshToken
        });
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Attempting to refresh a revoked token must fail
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            Token = registered.Tokens.RefreshToken
        });
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Revoke token ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RevokeToken_WithValidToken_ReturnsNoContent()
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            TenantName = "Revoke Clinic",
            FullName = "Admin",
            Email = $"revoke2-{Guid.NewGuid()}@test.com",
            Password = "Password123!"
        });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();

        var response = await _client.PostAsJsonAsync("/api/auth/revoke-token", new
        {
            Token = registered!.Tokens.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RevokeToken_WithInvalidToken_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/revoke-token", new
        {
            Token = "not-a-real-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────
    private record TokenResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);
    private record RegisterResponse(Guid TenantId, Guid UserId, TokenResponseDto Tokens);
}
