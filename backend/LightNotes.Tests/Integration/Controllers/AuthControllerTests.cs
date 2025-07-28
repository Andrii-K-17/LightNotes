using System.Net;
using System.Net.Http.Json;
using LightNotes.Application.DTOs.Auth;
using LightNotes.Tests.Integration;
using Xunit;

namespace LightNotes.Tests.Integration.Controllers;

public class AuthControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string CreateUniqueEmail() => $"{Guid.NewGuid()}@example.com";

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Register_ReturnsOkOrBadRequest_DependsOnEmailExistence(bool emailExists)
    {
        var email = CreateUniqueEmail();
        var request = new RegisterRequestDto
        {
            Email = email,
            Password = "Password123!",
            Name = "Test User"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        if (emailExists)
        {
            var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", request);
            Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
        }
        else
        {
            var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(content);
            Assert.Equal(request.Email, content.Email);
            Assert.False(string.IsNullOrWhiteSpace(content.Token));
        }
    }

    [Theory]
    [InlineData("nonexistent@example.com", "wrongpassword", false)]
    [InlineData(null, "ValidPassword1!", true)]
    public async Task Login_ReturnsUnauthorizedOrOk_DependsOnCredentials(string? email, string password, bool areCredentialsValid)
    {
        var testEmail = email ?? CreateUniqueEmail();

        var registerRequest = new RegisterRequestDto
        {
            Email = testEmail,
            Password = password,
            Name = "Test User"
        };

        if (areCredentialsValid)
        {
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        }

        var loginRequest = new LoginRequestDto
        {
            Email = testEmail,
            Password = password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        if (areCredentialsValid)
        {
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var content = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(content);
            Assert.Equal(testEmail, content.Email);
            Assert.False(string.IsNullOrWhiteSpace(content.Token));
        }
        else
        {
            Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
        }
    }

    [Theory]
    [InlineData("invalid-email", "Password123!")]
    [InlineData("email@example.com", "123")]
    [InlineData("", "")]
    public async Task Login_ReturnsBadRequest_DependsOnRequestData(string email, string password)
    {
        var request = new LoginRequestDto
        {
            Email = email,
            Password = password
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("invalid-email", "Password123!", "Test User")]
    [InlineData("email@example.com", "123", "Test User")]
    [InlineData("email@example.com", "Password123!", "TOO_LONG_NAME")]
    [InlineData("", "", "")]
    public async Task Register_ReturnsBadRequest_DependsOnRequestData(string email, string password, string name)
    {
        var actualName = name == "TOO_LONG_NAME" ? new string('A', 300) : name;
        var request = new RegisterRequestDto
        {
            Email = email,
            Password = password,
            Name = actualName
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
