using LightNotes.Domain.Enums;

namespace LightNotes.Domain.Entities;

/// <summary>
/// Учасник нотатки
/// </summary>
public class NoteCollaborator : BaseEntity
{
    public Guid NoteId { get; set; } // Зовнішній ключ до нотатки
    public Guid UserId { get; set; } // Зовнішній ключ до користувача
    public Role Role { get; set; } // Роль користувача

    // Навігаційні властивості
    public Note Note { get; set; } = null!;
    public User User { get; set; } = null!;
}
