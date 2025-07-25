using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using LightNotes.Infrastructure.Services.Auth;
using LightNotes.Infrastructure.Data;
using LightNotes.Application.DTOs.Auth;
using LightNotes.Domain.Entities;

namespace LightNotes.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly Mock<IConfiguration> _mockConfig = new();
    private readonly Mock<ILogger<AuthService>> _mockLogger = new();

    public AuthServiceTests()
    {
        _mockConfig.Setup(c => c["Jwt:Secret"]).Returns("supersecretkeysupersecretkey12345678123456789");
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
    }

    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task RegisterAsync_ReturnsAuthResponse_WhenEmailIsNew()
    {
        var context = new ApplicationDbContext(CreateOptions());
        var service = new AuthService(context, _mockConfig.Object, _mockLogger.Object);

        var request = new RegisterRequestDto
        {
            Email = "new@example.com",
            Password = "Password123",
            Name = "New User"
        };

        var result = await service.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Name, result.Name);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task RegisterAsync_ReturnsNull_WhenEmailExists()
    {
        var context = new ApplicationDbContext(CreateOptions());

        context.Users.Add(new User
        {
            Email = "existing@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("any"),
            Name = "Existing"
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, _mockConfig.Object, _mockLogger.Object);

        var request = new RegisterRequestDto
        {
            Email = "existing@example.com",
            Password = "NewPassword123",
            Name = "Existing User"
        };

        var result = await service.RegisterAsync(request);

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsAuthResponse_WhenCredentialsAreValid()
    {
        var context = new ApplicationDbContext(CreateOptions());

        var password = "ValidPass123";
        var user = new User
        {
            Email = "valid@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Name = "Valid User"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _mockConfig.Object, _mockLogger.Object);

        var request = new LoginRequestDto
        {
            Email = user.Email,
            Password = password
        };

        var result = await service.LoginAsync(request);

        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Name, result.Name);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenUserNotFound()
    {
        var context = new ApplicationDbContext(CreateOptions());
        var service = new AuthService(context, _mockConfig.Object, _mockLogger.Object);

        var request = new LoginRequestDto
        {
            Email = "missing@example.com",
            Password = "password"
        };

        var result = await service.LoginAsync(request);

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordIsWrong()
    {
        var context = new ApplicationDbContext(CreateOptions());

        context.Users.Add(new User
        {
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct"),
            Name = "Test"
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, _mockConfig.Object, _mockLogger.Object);

        var request = new LoginRequestDto
        {
            Email = "user@example.com",
            Password = "wrong"
        };

        var result = await service.LoginAsync(request);

        Assert.Null(result);
    }
}
