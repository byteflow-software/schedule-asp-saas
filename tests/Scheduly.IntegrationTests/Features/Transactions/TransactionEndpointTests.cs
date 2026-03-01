using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Scheduly.IntegrationTests.Helpers;

namespace Scheduly.IntegrationTests.Features.Transactions;

public class TransactionEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TransactionEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTransactions_Returns200()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        var response = await _client.GetAsync("/api/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("items").GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetTransactionSummary_Returns200()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        var response = await _client.GetAsync("/api/transactions/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Should return some summary object even if empty
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.ValueKind.Should().Be(JsonValueKind.Object);
    }
}
