namespace LightNotes.Application.DTOs.Auth;

/// <summary>
/// Дані для запиту на вхід користувача
/// </summary>
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
