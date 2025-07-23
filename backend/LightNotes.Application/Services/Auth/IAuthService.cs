using LightNotes.Application.DTOs.Auth;

namespace LightNotes.Application.Services.Auth;

/// <summary>
/// Сервіс для реєстрації та авторизації користувачів
/// </summary>
public interface IAuthService
{
    // Реєстрація нового користувача
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request);

    // Логін існуючого користувача
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
}
