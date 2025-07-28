using System.ComponentModel.DataAnnotations;

namespace LightNotes.Application.DTOs.Chat;

/// <summary>
/// Дані для запиту на відправлення нового повідомлення чату
/// </summary>
public class SendMessageRequestDto
{
    [Required]
    [MaxLength(500, ErrorMessage = "Повідомлення не повинно перевищувати 500 символів.")]
    public string Text { get; set; } = string.Empty;
}
