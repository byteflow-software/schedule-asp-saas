using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Scheduly.IntegrationTests.Helpers;

namespace Scheduly.IntegrationTests.Features.Vacancies;

public class VacancyEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public VacancyEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateVacancy_ValidData_Returns201()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);
        var userId = await AuthHelper.CreateUserAsync(_client);
        var serviceId = await AuthHelper.CreateServiceAsync(_client);

        var startTime = DateTime.UtcNow.AddDays(1).Date.AddHours(9);
        var endTime = startTime.AddHours(1);

        var response = await _client.PostAsJsonAsync("/api/vacancies", new
        {
            UserId = userId,
            ServiceId = serviceId,
            StartTime = startTime,
            EndTime = endTime
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, because: body);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task BulkCreateVacancies_Returns201()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);
        var userId = await AuthHelper.CreateUserAsync(_client);
        var serviceId = await AuthHelper.CreateServiceAsync(_client);

        var baseDate = DateTime.UtcNow.AddDays(2).Date.AddHours(9);

        var response = await _client.PostAsJsonAsync("/api/vacancies/bulk", new
        {
            UserId = userId,
            ServiceId = serviceId,
            Slots = new[]
            {
                new { StartTime = baseDate, EndTime = baseDate.AddHours(1) },
                new { StartTime = baseDate.AddHours(1), EndTime = baseDate.AddHours(2) }
            }
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, because: body);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task GetVacancies_Returns200()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);
        var userId = await AuthHelper.CreateUserAsync(_client);
        var serviceId = await AuthHelper.CreateServiceAsync(_client);

        var startTime = DateTime.UtcNow.AddDays(3).Date.AddHours(9);
        await _client.PostAsJsonAsync("/api/vacancies", new
        {
            UserId = userId,
            ServiceId = serviceId,
            StartTime = startTime,
            EndTime = startTime.AddHours(1)
        });

        var response = await _client.GetAsync("/api/vacancies");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task DeleteVacancy_Returns204()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);
        var userId = await AuthHelper.CreateUserAsync(_client);
        var serviceId = await AuthHelper.CreateServiceAsync(_client);

        var startTime = DateTime.UtcNow.AddDays(4).Date.AddHours(9);
        var createResponse = await _client.PostAsJsonAsync("/api/vacancies", new
        {
            UserId = userId,
            ServiceId = serviceId,
            StartTime = startTime,
            EndTime = startTime.AddHours(1)
        });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var vacancyId = created.GetProperty("id").GetString();

        var response = await _client.DeleteAsync($"/api/vacancies/{vacancyId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
