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
            .UseSqlite($"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared")
            .Options;
    }

    private static ApplicationDbContext CreateContext(DbContextOptions<ApplicationDbContext> options)
    {
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static async Task<User> CreateTestUserAsync(ApplicationDbContext context, string? password = "", string? email = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Email = email ?? $"{Guid.NewGuid()}@example.com",
            Name = "Test User"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RegisterAsync_ReturnsNullOrAuthResponse_DependsOnEmailExistence(bool emailExists)
    {
        var options = CreateOptions();
        await using var context = CreateContext(options);
        var existingUser = await CreateTestUserAsync(context, "existing@example.com");
        var service = new AuthService(context, _mockConfig.Object, _mockLogger.Object);

        var request = new RegisterRequestDto
        {
            Email = emailExists ? existingUser.Email : "new@example.com",
            Password = "Password123",
            Name = "New User"
        };

        var result = await service.RegisterAsync(request);

        if (emailExists)
        {
            Assert.Null(result);
        }
        else
        {
            Assert.NotNull(result);
            Assert.Equal(request.Email, result.Email);
            Assert.Equal(request.Name, result.Name);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
        }
    }

    [Fact]
    public async Task LoginAsync_ReturnsAuthResponse_WhenCredentialsAreValid()
    {
        var options = CreateOptions();
        await using var context = CreateContext(options);

        var password = "ValidPassword123";

        var user = await CreateTestUserAsync(context, password: "ValidPassword123");

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

    [Theory]
    [InlineData("Password is wrong", "user@example.com", "wrong", "correct_password")]
    [InlineData("User not found", "missing@example.com", "password", null)]
    public async Task LoginAsync_ReturnsNull_WhenPasswordIsWrongOrUserNotFound(string condition, string email, string inputPassword, string? actualPassword)
    {
        var options = CreateOptions();
        await using var context = CreateContext(options);
        var service = new AuthService(context, _mockConfig.Object, _mockLogger.Object);

        if (condition == "Password is wrong")
            await CreateTestUserAsync(context, email, actualPassword);

        var request = new LoginRequestDto
        {
            Email = email,
            Password = inputPassword
        };

        var result = await service.LoginAsync(request);

        Assert.Null(result);
    }
}
