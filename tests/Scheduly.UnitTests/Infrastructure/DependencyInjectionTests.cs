using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Infrastructure;

namespace Scheduly.UnitTests.Infrastructure;

public class DependencyInjectionTests
{
    private static IConfiguration CreateConfig(bool includeConnectionString = false)
    {
        var dict = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "ThisIsASecretKeyForTestingThatMustBeAtLeast32Chars!",
            ["Jwt:Issuer"] = "Test",
            ["Jwt:Audience"] = "Test",
            ["Jwt:AccessTokenExpirationMinutes"] = "30",
            ["Jwt:RefreshTokenExpirationDays"] = "14",
            ["Email:Host"] = "localhost",
            ["Email:Port"] = "1025",
            ["Asaas:PlatformWalletId"] = "wallet_test",
            ["Asaas:PlatformSplitPercent"] = "3"
        };

        if (includeConnectionString)
            dict["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;";

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    [Fact]
    public void AddInfrastructure_NoDatabaseNoHangfire_RegistersCoreServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(CreateConfig(), useHangfire: false, useDatabase: false);

        var provider = services.BuildServiceProvider();

        provider.GetService<ICurrentTenantService>().Should().NotBeNull();
        provider.GetService<ICurrentUserService>().Should().NotBeNull();
        provider.GetService<IPasswordHasher>().Should().NotBeNull();
        provider.GetService<IDateTimeProvider>().Should().NotBeNull();
        provider.GetService<INotificationService>().Should().NotBeNull();
        provider.GetService<IEmailService>().Should().NotBeNull();
        provider.GetService<IJwtTokenService>().Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_NoConnectionString_NoDatabaseFlag_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var act = () => services.AddInfrastructure(CreateConfig(), useHangfire: false, useDatabase: false);

        act.Should().NotThrow();
    }

    [Fact]
    public void AddInfrastructure_NoConnectionString_WithDatabaseFlag_Throws()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var act = () => services.AddInfrastructure(CreateConfig(), useHangfire: false, useDatabase: true);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Connection string*");
    }

    [Fact]
    public void AddInfrastructure_WithConnectionString_RegistersDbContext()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(CreateConfig(includeConnectionString: true),
            useHangfire: false, useDatabase: true);

        var descriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IApplicationDbContext));

        descriptor.Should().NotBeNull();
    }
}
