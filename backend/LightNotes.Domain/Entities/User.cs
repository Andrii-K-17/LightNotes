using System.ComponentModel.DataAnnotations;

namespace LightNotes.Domain.Entities;

/// <summary>
/// Сутність користувача
/// </summary>
public class User : BaseEntity
{
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Note> OwnedNotes { get; set; } = new HashSet<Note>();
    public ICollection<NoteCollaborator> Collaborations { get; set; } = new HashSet<NoteCollaborator>();
    public ICollection<ChatMessage> SentMessages { get; set; } = new HashSet<ChatMessage>();
}
