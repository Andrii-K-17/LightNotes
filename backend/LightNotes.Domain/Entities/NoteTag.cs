namespace LightNotes.Domain.Entities;

/// <summary>
/// Тег нотатки
/// </summary>
public class NoteTag : BaseEntity
{
    public Guid NoteId { get; set; } // Зовнішній ключ до нотатки
    public string Tag { get; set; } = string.Empty; // Текст тегу

    // Навігаційна властивість
    public Note Note { get; set; } = null!;
}
