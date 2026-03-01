using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Scheduly.Infrastructure.Identity;

namespace Scheduly.UnitTests.Infrastructure.Identity;

public class CurrentUserServiceTests
{
    [Fact]
    public void UserId_WithValidClaim_ReturnsGuid()
    {
        var userId = Guid.NewGuid();
        var accessor = CreateAccessor(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

        var sut = new CurrentUserService(accessor);

        sut.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserId_WithoutClaim_ReturnsNull()
    {
        var accessor = CreateAccessor();
        var sut = new CurrentUserService(accessor);

        sut.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_WithInvalidGuid_ReturnsNull()
    {
        var accessor = CreateAccessor(new Claim(ClaimTypes.NameIdentifier, "not-a-guid"));
        var sut = new CurrentUserService(accessor);

        sut.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_WithNullHttpContext_ReturnsNull()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var sut = new CurrentUserService(accessor);

        sut.UserId.Should().BeNull();
    }

    [Fact]
    public void Role_WithValidClaim_ReturnsRole()
    {
        var accessor = CreateAccessor(new Claim(ClaimTypes.Role, "Admin"));
        var sut = new CurrentUserService(accessor);

        sut.Role.Should().Be("Admin");
    }

    [Fact]
    public void Role_WithoutClaim_ReturnsNull()
    {
        var accessor = CreateAccessor();
        var sut = new CurrentUserService(accessor);

        sut.Role.Should().BeNull();
    }

    private static IHttpContextAccessor CreateAccessor(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = principal };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);
        return accessor;
    }
}
