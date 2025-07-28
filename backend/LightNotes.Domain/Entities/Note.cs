using System.ComponentModel.DataAnnotations;

namespace LightNotes.Domain.Entities;

/// <summary>
/// Нотатка, яку створює користувач
/// </summary>
public class Note : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Color { get; set; }

    public Guid OwnerId { get; set; }

    public bool IsPinned { get; set; } = false;
    public bool IsArchived { get; set; } = false;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReminderAt { get; set; } // Час нагадування

    public User Owner { get; set; } = null!;
    public ICollection<NoteCollaborator> Collaborators { get; set; } = new HashSet<NoteCollaborator>();
    public ICollection<NoteTag> Tags { get; set; } = new HashSet<NoteTag>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new HashSet<ChatMessage>();
}
