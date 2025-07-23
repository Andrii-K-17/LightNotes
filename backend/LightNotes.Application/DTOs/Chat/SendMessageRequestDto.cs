namespace LightNotes.Application.DTOs.Chat;

/// <summary>
/// Дані для запиту на відправлення нового повідомлення чату
/// </summary>
public class SendMessageRequestDto
{
    public string Text { get; set; } = string.Empty;
}
