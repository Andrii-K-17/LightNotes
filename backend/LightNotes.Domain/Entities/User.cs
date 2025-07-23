namespace LightNotes.Domain.Entities;

/// <summary>
/// Сутність користувача
/// </summary>
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty; // Унікальний email
    public string PasswordHash { get; set; } = string.Empty; // Хеш пароля
    public string Name { get; set; } = string.Empty; // Ім’я користувача

    // Навігаційні властивості
    public ICollection<Note> OwnedNotes { get; set; } = new HashSet<Note>();
    public ICollection<NoteCollaborator> Collaborations { get; set; } = new HashSet<NoteCollaborator>();
    public ICollection<ChatMessage> SentMessages { get; set; } = new HashSet<ChatMessage>();
}
