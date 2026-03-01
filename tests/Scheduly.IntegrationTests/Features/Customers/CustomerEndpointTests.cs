using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Scheduly.IntegrationTests.Helpers;

namespace Scheduly.IntegrationTests.Features.Customers;

public class CustomerEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CustomerEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCustomer_ValidData_Returns201()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        var response = await _client.PostAsJsonAsync("/api/customers", new
        {
            FullName = "Maria Silva",
            Email = $"maria-{Guid.NewGuid()}@test.com",
            Phone = "+5511999990001",
            CpfCnpj = "12345678901"
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, because: body);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("fullName").GetString().Should().Be("Maria Silva");
    }

    [Fact]
    public async Task GetCustomers_Returns200()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        // Create a customer first
        await _client.PostAsJsonAsync("/api/customers", new
        {
            FullName = "Joao Souza",
            Email = $"joao-{Guid.NewGuid()}@test.com",
            Phone = "+5511999990002",
            CpfCnpj = "98765432100"
        });

        var response = await _client.GetAsync("/api/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("items").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetCustomerById_Returns200()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        var createResponse = await _client.PostAsJsonAsync("/api/customers", new
        {
            FullName = "Ana Pereira",
            Email = $"ana-{Guid.NewGuid()}@test.com",
            Phone = "+5511999990003",
            CpfCnpj = "11122233344"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var customerId = created.GetProperty("id").GetString();

        var response = await _client.GetAsync($"/api/customers/{customerId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("fullName").GetString().Should().Be("Ana Pereira");
    }

    [Fact]
    public async Task UpdateCustomer_Returns200()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        var createResponse = await _client.PostAsJsonAsync("/api/customers", new
        {
            FullName = "Carlos Lima",
            Email = $"carlos-{Guid.NewGuid()}@test.com",
            Phone = "+5511999990004",
            CpfCnpj = "55566677788"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var customerId = created.GetProperty("id").GetString();

        var response = await _client.PutAsJsonAsync($"/api/customers/{customerId}", new
        {
            Id = customerId,
            FullName = "Carlos Lima Jr",
            Email = $"carlos-jr-{Guid.NewGuid()}@test.com",
            Phone = "+5511999990005",
            CpfCnpj = "55566677788"
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: body);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("fullName").GetString().Should().Be("Carlos Lima Jr");
    }

    [Fact]
    public async Task DeleteCustomer_Returns204()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        var createResponse = await _client.PostAsJsonAsync("/api/customers", new
        {
            FullName = "Delete Me",
            Email = $"delete-{Guid.NewGuid()}@test.com",
            Phone = "+5511999990006",
            CpfCnpj = "99988877766"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var customerId = created.GetProperty("id").GetString();

        var response = await _client.DeleteAsync($"/api/customers/{customerId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
