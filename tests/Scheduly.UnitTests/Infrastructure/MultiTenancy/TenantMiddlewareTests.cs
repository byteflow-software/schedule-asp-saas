using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Scheduly.Infrastructure.MultiTenancy;

namespace Scheduly.UnitTests.Infrastructure.MultiTenancy;

public class TenantMiddlewareTests
{
    private readonly CurrentTenantService _tenantService = new();
    private bool _nextCalled;

    private TenantMiddleware CreateMiddleware()
    {
        _nextCalled = false;
        return new TenantMiddleware(_ =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        });
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/register")]
    [InlineData("/api/auth/refresh-token")]
    [InlineData("/api/auth/revoke-token")]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    [InlineData("/health/live")]
    [InlineData("/swagger")]
    public async Task InvokeAsync_ExemptPath_SkipsTenantResolution(string path)
    {
        var middleware = CreateMiddleware();
        var context = CreateContext(path);

        await middleware.InvokeAsync(context, _tenantService);

        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithTenantHeader_SetsTenantId()
    {
        var middleware = CreateMiddleware();
        var tenantId = Guid.NewGuid();
        var context = CreateContext("/api/services");
        context.Request.Headers["X-Tenant-Id"] = tenantId.ToString();

        await middleware.InvokeAsync(context, _tenantService);

        _nextCalled.Should().BeTrue();
        _tenantService.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task InvokeAsync_TenantMismatchWithJwt_Returns403()
    {
        var middleware = CreateMiddleware();
        var headerTenantId = Guid.NewGuid();
        var claimTenantId = Guid.NewGuid();

        var context = CreateContext("/api/services",
            new Claim("tenant_id", claimTenantId.ToString()));
        context.Request.Headers["X-Tenant-Id"] = headerTenantId.ToString();

        await middleware.InvokeAsync(context, _tenantService);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_TenantFromJwtClaim_SetsTenantId()
    {
        var middleware = CreateMiddleware();
        var tenantId = Guid.NewGuid();
        var context = CreateContext("/api/services",
            new Claim("tenant_id", tenantId.ToString()));

        await middleware.InvokeAsync(context, _tenantService);

        _nextCalled.Should().BeTrue();
        _tenantService.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task InvokeAsync_NoTenantInfo_Returns400()
    {
        var middleware = CreateMiddleware();
        var context = CreateContext("/api/services");

        await middleware.InvokeAsync(context, _tenantService);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task InvokeAsync_InvalidTenantHeaderGuid_FallsToJwt()
    {
        var middleware = CreateMiddleware();
        var tenantId = Guid.NewGuid();
        var context = CreateContext("/api/services",
            new Claim("tenant_id", tenantId.ToString()));
        context.Request.Headers["X-Tenant-Id"] = "not-a-guid";

        await middleware.InvokeAsync(context, _tenantService);

        _nextCalled.Should().BeTrue();
        _tenantService.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task InvokeAsync_TenantHeaderMatchesJwtClaim_Succeeds()
    {
        var middleware = CreateMiddleware();
        var tenantId = Guid.NewGuid();
        var context = CreateContext("/api/services",
            new Claim("tenant_id", tenantId.ToString()));
        context.Request.Headers["X-Tenant-Id"] = tenantId.ToString();

        await middleware.InvokeAsync(context, _tenantService);

        _nextCalled.Should().BeTrue();
        _tenantService.TenantId.Should().Be(tenantId);
    }

    private static DefaultHttpContext CreateContext(string path, params Claim[] claims)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        if (claims.Length > 0)
        {
            var identity = new ClaimsIdentity(claims, "Test");
            context.User = new ClaimsPrincipal(identity);
        }
        return context;
    }
}
