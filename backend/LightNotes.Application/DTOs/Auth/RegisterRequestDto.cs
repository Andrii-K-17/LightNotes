using System.ComponentModel.DataAnnotations;

namespace LightNotes.Application.DTOs.Auth;

/// <summary>
/// Дані для реєстрації нового користувача
/// </summary>
public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Пароль повинен містити щонайменше 8 символів.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
