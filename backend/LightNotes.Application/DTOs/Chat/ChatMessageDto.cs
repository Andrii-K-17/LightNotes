using System;

namespace LightNotes.Application.DTOs.Chat;

/// <summary>
/// DTO для передачі даних повідомлення чату
/// </summary>
public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid NoteId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
