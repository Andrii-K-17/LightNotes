namespace LightNotes.Domain.Entities;

/// <summary>
/// Нотатка, яку створює користувач
/// </summary>
public class Note : BaseEntity
{
    public string Title { get; set; } = string.Empty; // Заголовок нотатки
    public string Content { get; set; } = string.Empty; // Зміст нотатки
    public string? Color { get; set; } // Колір нотатки
    public Guid OwnerId { get; set; } // Id власника нотатки
    public bool IsPinned { get; set; } = false; // Закріплена нотатка
    public bool IsArchived { get; set; } = false; // Архівована нотатка
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Час останнього оновлення
    public DateTime? ReminderAt { get; set; } // Час нагадування

    // Навігаційні властивості
    public User Owner { get; set; } = null!;
    public ICollection<NoteCollaborator> Collaborators { get; set; } = new HashSet<NoteCollaborator>();
    public ICollection<NoteTag> Tags { get; set; } = new HashSet<NoteTag>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new HashSet<ChatMessage>();
}
