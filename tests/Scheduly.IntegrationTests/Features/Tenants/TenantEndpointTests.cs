using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Scheduly.IntegrationTests.Helpers;

namespace Scheduly.IntegrationTests.Features.Tenants;

public class TenantEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TenantEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCurrentTenant_Returns200()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        var response = await _client.GetAsync("/api/tenants/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("name").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateCurrentTenant_Returns200()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        var response = await _client.PutAsJsonAsync("/api/tenants/me", new
        {
            Name = "Updated Clinic Name",
            CpfCnpj = "12345678000190",
            Phone = "+5511999998888",
            Email = "clinic@test.com",
            Address = "Rua Principal",
            AddressNumber = "123",
            Complement = "Sala 1",
            Neighborhood = "Centro",
            City = "Sao Paulo",
            State = "SP",
            PostalCode = "01001000",
            LogoUrl = (string?)null
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("name").GetString().Should().Be("Updated Clinic Name");
    }
}
