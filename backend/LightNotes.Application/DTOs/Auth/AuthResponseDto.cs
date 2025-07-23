namespace LightNotes.Application.DTOs.Auth;

/// <summary>
/// DTO-відповідь, що повертається після логіну або реєстрації
/// Містить дані користувача та JWT токен
/// </summary>
public class AuthResponseDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
