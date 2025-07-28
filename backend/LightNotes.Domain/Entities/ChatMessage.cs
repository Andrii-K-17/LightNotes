using System.ComponentModel.DataAnnotations;

namespace LightNotes.Domain.Entities;

/// <summary>
/// Повідомлення чату, прив’язане до нотатки
/// </summary>
public class ChatMessage : BaseEntity
{
    public Guid NoteId { get; set; }
    public Guid SenderId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Note Note { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
