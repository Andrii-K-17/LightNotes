namespace LightNotes.Application.DTOs.Auth;

/// <summary>
/// Дані для реєстрації нового користувача
/// </summary>
public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
