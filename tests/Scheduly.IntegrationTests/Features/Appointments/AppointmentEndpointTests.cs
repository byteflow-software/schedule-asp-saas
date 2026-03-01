using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Scheduly.IntegrationTests.Helpers;

namespace Scheduly.IntegrationTests.Features.Appointments;

public class AppointmentEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AppointmentEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(Guid CustomerId, Guid UserId, Guid ServiceId)> SetupDependenciesAsync()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);
        var customerId = await AuthHelper.CreateCustomerAsync(_client);
        var userId = await AuthHelper.CreateUserAsync(_client);
        var serviceId = await AuthHelper.CreateServiceAsync(_client);
        return (customerId, userId, serviceId);
    }

    [Fact]
    public async Task CreateAppointment_ValidData_Returns201()
    {
        var (customerId, userId, serviceId) = await SetupDependenciesAsync();

        var startTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var endTime = startTime.AddHours(1);

        var response = await _client.PostAsJsonAsync("/api/appointments", new
        {
            CustomerId = customerId,
            UserId = userId,
            ServiceId = serviceId,
            VacancyId = (Guid?)null,
            StartTime = startTime,
            EndTime = endTime,
            Notes = "Integration test appointment"
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, because: body);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAppointments_Returns200()
    {
        var (customerId, userId, serviceId) = await SetupDependenciesAsync();

        var startTime = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
        await _client.PostAsJsonAsync("/api/appointments", new
        {
            CustomerId = customerId,
            UserId = userId,
            ServiceId = serviceId,
            VacancyId = (Guid?)null,
            StartTime = startTime,
            EndTime = startTime.AddHours(1),
            Notes = (string?)null
        });

        var response = await _client.GetAsync("/api/appointments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("items").GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetAppointmentById_Returns200()
    {
        var (customerId, userId, serviceId) = await SetupDependenciesAsync();

        var startTime = DateTime.UtcNow.AddDays(3).Date.AddHours(10);
        var createResponse = await _client.PostAsJsonAsync("/api/appointments", new
        {
            CustomerId = customerId,
            UserId = userId,
            ServiceId = serviceId,
            VacancyId = (Guid?)null,
            StartTime = startTime,
            EndTime = startTime.AddHours(1),
            Notes = "Get by id test"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var appointmentId = created.GetProperty("id").GetString();

        var response = await _client.GetAsync($"/api/appointments/{appointmentId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("id").GetString().Should().Be(appointmentId);
    }

    [Fact]
    public async Task CancelAppointment_Returns204()
    {
        var (customerId, userId, serviceId) = await SetupDependenciesAsync();

        var startTime = DateTime.UtcNow.AddDays(4).Date.AddHours(10);
        var createResponse = await _client.PostAsJsonAsync("/api/appointments", new
        {
            CustomerId = customerId,
            UserId = userId,
            ServiceId = serviceId,
            VacancyId = (Guid?)null,
            StartTime = startTime,
            EndTime = startTime.AddHours(1),
            Notes = "To be cancelled"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var appointmentId = created.GetProperty("id").GetString();

        var response = await _client.PatchAsync($"/api/appointments/{appointmentId}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
