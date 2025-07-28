using System.ComponentModel.DataAnnotations;

namespace LightNotes.Application.DTOs.Auth;

/// <summary>
/// Дані для запиту на вхід користувача
/// </summary>
public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Пароль повинен містити щонайменше 8 символів.")]
    public string Password { get; set; } = string.Empty;
}
