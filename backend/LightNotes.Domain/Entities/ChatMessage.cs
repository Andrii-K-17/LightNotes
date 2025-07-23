namespace LightNotes.Domain.Entities;

/// <summary>
/// Повідомлення чату, прив’язане до нотатки
/// </summary>
public class ChatMessage : BaseEntity
{
    public Guid NoteId { get; set; } // Зовнішній ключ до нотатки
    public Guid SenderId { get; set; } // Зовнішній ключ до користувача
    public string Text { get; set; } = string.Empty; // Текст повідомлення
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Час відправлення

    // Навігаційні властивості
    public Note Note { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
