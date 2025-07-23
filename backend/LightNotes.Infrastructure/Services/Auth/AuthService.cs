using LightNotes.Application.DTOs.Auth;
using LightNotes.Application.Services.Auth;
using LightNotes.Domain.Entities;
using LightNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LightNotes.Infrastructure.Services.Auth;

/// <summary>
/// Реалізація сервісу аутентифікації
/// </summary>
/// <param name="context">Контекст бази даних</param>
/// <param name="configuration">Конфігурація (appsettings.json)</param>
/// <param name="logger">Сервіс логування</param>
public class AuthService(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthService> logger) : IAuthService
{
    private readonly ApplicationDbContext _context = context; // Контекст бази даних
    private readonly IConfiguration _configuration = configuration; // Для зчитування конфігурації (appsettings.json)
    private readonly ILogger<AuthService> _logger = logger; // Логування

    // Метод реєстрації користувача
    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request)
    {
        _logger.LogInformation("Спроба реєстрації нового користувача з Email: {Email}", request.Email);

        // Перевірка, чи існує користувач з таким email
        bool userExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (userExists)
        {
            _logger.LogWarning("Спроба реєстрації користувача з існуючим Email: {Email}", request.Email);
            return null;
        }

        // Хешування пароля за допомогою BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Створення нового об'єкта користувача
        var newUser = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Name = request.Name
        };

        try
        {
            // Додавання нового користувача до БД
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Користувач {Email} успішно зареєстрований. ID: {UserId}", newUser.Email, newUser.Id);

            // Генерація JWT токена для новоствореного користувача
            var token = GenerateJwtToken(newUser);

            // Повернення DTO з даними користувача та токеном
            return new AuthResponseDto
            {
                UserId = newUser.Id,
                Name = newUser.Name,
                Email = newUser.Email,
                Token = token
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час реєстрації користувача {Email}.", request.Email);
            return null;
        }
    }

    // Метод входу користувача
    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
    {
        _logger.LogInformation("Спроба входу користувача з Email: {Email}", request.Email);

        // Пошук користувача по email
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            _logger.LogWarning("Спроба входу з неіснуючим Email: {Email}", request.Email);
            return null;
        }

        // Перевірка відповідності пароля
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Невірний пароль для користувача: {Email}", request.Email);
            return null;
        }

        try
        {
            // Генерація JWT токена після успішної авторизації
            string token = GenerateJwtToken(user);

            _logger.LogInformation("Користувач {Email} успішно увійшов. ID: {UserId}", user.Email, user.Id);

            // Повертаємо DTO з даними та токеном
            return new AuthResponseDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час генерації токена для користувача {Email}.", request.Email);
            return null;
        }
    }

    // Генерує JWT токен для користувача
    private string GenerateJwtToken(User user)
    {
        var tokenDescriptor = BuildTokenDescriptor(user); // Описуємо, що містить токен
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor); // Створюємо токен
        return tokenHandler.WriteToken(token); // Повертаємо у форматі рядка
    }

    // Формує опис JWT токена (claims, ключ, термін дії, підпис)
    private SecurityTokenDescriptor BuildTokenDescriptor(User user)
    {
        var jwtSecret = _configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(jwtSecret))
        {
            _logger.LogCritical("JWT Secret не налаштований. Перевірте appsettings.json.");
            throw new InvalidOperationException("JWT Secret not configured.");
        }

        // Дані, які будуть закодовані в токен
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = creds,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };
    }
}
