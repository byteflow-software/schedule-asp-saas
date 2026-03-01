using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Scheduly.IntegrationTests.Helpers;

namespace Scheduly.IntegrationTests.Features.Users;

public class UserEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_Returns200()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<JsonElement>();
        // At least the admin user created during registration
        users.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task CreateUser_ValidData_Returns201()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        var response = await _client.PostAsJsonAsync("/api/users", new
        {
            FullName = "Staff Member",
            Email = $"staff-{Guid.NewGuid()}@test.com",
            Password = "Password123!",
            Role = "Staff"
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, because: body);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        result.GetProperty("fullName").GetString().Should().Be("Staff Member");
        result.GetProperty("role").GetString().Should().Be("Staff");
    }

    [Fact]
    public async Task DeactivateUser_Returns204()
    {
        await AuthHelper.RegisterAndAuthAsync(_client);

        // Create a staff user to deactivate
        var createResponse = await _client.PostAsJsonAsync("/api/users", new
        {
            FullName = "To Deactivate",
            Email = $"deactivate-{Guid.NewGuid()}@test.com",
            Password = "Password123!",
            Role = "Staff"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var userId = created.GetProperty("id").GetString();

        var response = await _client.PatchAsync($"/api/users/{userId}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
