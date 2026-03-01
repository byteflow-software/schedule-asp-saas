using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Scheduly.IntegrationTests.Helpers;

namespace Scheduly.IntegrationTests.Features.Services;

public class ServiceEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ServiceEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateService_ValidData_Returns201()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        var response = await _client.PostAsJsonAsync("/api/services", new
        {
            Name = "Haircut",
            Description = "Standard haircut",
            DurationMinutes = 30,
            PriceInCents = 3500
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, because: body);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("name").GetString().Should().Be("Haircut");
        result.GetProperty("durationMinutes").GetInt32().Should().Be(30);
        result.GetProperty("priceInCents").GetInt32().Should().Be(3500);
    }

    [Fact]
    public async Task GetServices_Returns200()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        // Create a service first
        await _client.PostAsJsonAsync("/api/services", new
        {
            Name = "Massage",
            Description = "Relaxing massage",
            DurationMinutes = 60,
            PriceInCents = 8000
        });

        var response = await _client.GetAsync("/api/services");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var services = await response.Content.ReadFromJsonAsync<JsonElement>();
        services.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetServiceById_Returns200()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        // Create a service
        var createResponse = await _client.PostAsJsonAsync("/api/services", new
        {
            Name = "Facial",
            Description = "Deep cleansing facial",
            DurationMinutes = 45,
            PriceInCents = 6000
        });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var serviceId = created.GetProperty("id").GetString();

        var response = await _client.GetAsync($"/api/services/{serviceId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("name").GetString().Should().Be("Facial");
    }

    [Fact]
    public async Task UpdateService_Returns200()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        // Create a service
        var createResponse = await _client.PostAsJsonAsync("/api/services", new
        {
            Name = "Manicure",
            Description = "Basic manicure",
            DurationMinutes = 30,
            PriceInCents = 2500
        });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var serviceId = created.GetProperty("id").GetString();

        // Update
        var response = await _client.PutAsJsonAsync($"/api/services/{serviceId}", new
        {
            Id = serviceId,
            Name = "Premium Manicure",
            Description = "Premium gel manicure",
            DurationMinutes = 45,
            PriceInCents = 4500,
            IsActive = true
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("name").GetString().Should().Be("Premium Manicure");
        result.GetProperty("priceInCents").GetInt32().Should().Be(4500);
    }

    [Fact]
    public async Task CreateService_Unauthenticated_Returns401()
    {
        // Do NOT register/authenticate
        var response = await _client.PostAsJsonAsync("/api/services", new
        {
            Name = "Test",
            Description = "Test",
            DurationMinutes = 30,
            PriceInCents = 1000
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
