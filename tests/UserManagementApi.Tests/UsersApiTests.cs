using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UserManagementApi.DTOs;

namespace UserManagementApi.Tests;

public class UsersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string ApiKey = "dev-secret-key-change-me";
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UsersApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
    }

    private static CreateUserRequest NewUser(string? email = null) => new()
    {
        FullName = "Test User",
        Email = email ?? $"{Guid.NewGuid():N}@example.com",
        Age = 30
    };

    [Fact]
    public async Task Request_without_api_key_returns_401()
    {
        var anonymous = _factory.CreateClient(); // no X-API-KEY header
        var res = await anonymous.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Request_with_wrong_api_key_returns_401()
    {
        var c = _factory.CreateClient();
        c.DefaultRequestHeaders.Add("X-API-KEY", "wrong");
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/users")).StatusCode);
    }

    [Fact]
    public async Task Health_endpoint_is_public()
    {
        var res = await _factory.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Create_then_get_returns_user()
    {
        var created = await _client.PostAsJsonAsync("/api/users", NewUser());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var body = await created.Content.ReadFromJsonAsync<UserResponse>();
        var fetched = await _client.GetAsync($"/api/users/{body!.Id}");
        Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);
    }

    [Fact]
    public async Task Create_with_invalid_data_returns_400()
    {
        var bad = new CreateUserRequest { FullName = "A", Email = "not-an-email", Age = 5 };
        var res = await _client.PostAsJsonAsync("/api/users", bad);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Duplicate_email_returns_409()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/users", NewUser(email));
        var res = await _client.PostAsJsonAsync("/api/users", NewUser(email.ToUpperInvariant()));
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
    }

    [Fact]
    public async Task Update_changes_user()
    {
        var created = await (await _client.PostAsJsonAsync("/api/users", NewUser()))
            .Content.ReadFromJsonAsync<UserResponse>();

        var update = new UpdateUserRequest { FullName = "Renamed", Email = created!.Email, Age = 41 };
        var res = await _client.PutAsJsonAsync($"/api/users/{created.Id}", update);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var updated = await res.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("Renamed", updated!.FullName);
    }

    [Fact]
    public async Task Delete_removes_user_and_second_delete_returns_404()
    {
        var created = await (await _client.PostAsJsonAsync("/api/users", NewUser()))
            .Content.ReadFromJsonAsync<UserResponse>();

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/users/{created!.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync($"/api/users/{created.Id}")).StatusCode);
    }
}
