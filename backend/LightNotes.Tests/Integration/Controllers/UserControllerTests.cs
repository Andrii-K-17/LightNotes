using System.Net;
using System.Net.Http.Json;
using LightNotes.Application.DTOs.Auth;
using LightNotes.Tests.Integration;
using Xunit;

namespace LightNotes.Tests.Integration.Controllers;

public class UserControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string CreateUniqueEmail() => $"{Guid.NewGuid()}@example.com";

    private async Task<HttpClient> CreateAuthorizedClientAsync()
    {
        var email = CreateUniqueEmail();
        var registerRequest = new RegisterRequestDto
        {
            Email = email,
            Password = "ValidPassword123!",
            Name = "Test User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var authData = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authData?.Token 
                                                     ?? throw new Exception("Token not received"));
        return client;
    }

    private static async Task<Guid> CreateTestUserAsync(HttpClient client)
    {
        var email = CreateUniqueEmail();
        var registerRequest = new RegisterRequestDto
        {
            Email = email,
            Password = "ValidPassword123!",
            Name = "To Delete"
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();

        var authData = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return authData?.UserId ?? throw new Exception("User not created");
    }

    [Theory]
    [InlineData(true, HttpStatusCode.NoContent)]
    [InlineData(false, HttpStatusCode.NotFound)]
    public async Task DeleteUserAccount_ReturnsExpectedStatus_BasedOnUserExistence(bool userExists, HttpStatusCode expectedStatus)
    {
        var client = await CreateAuthorizedClientAsync();

        Guid userId = userExists ? await CreateTestUserAsync(client) : Guid.NewGuid();

        var deleteResponse = await client.DeleteAsync($"/api/user/{userId}");

        Assert.Equal(expectedStatus, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteUserAccount_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        var userId = Guid.NewGuid();
        var response = await _client.DeleteAsync($"/api/user/{userId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
