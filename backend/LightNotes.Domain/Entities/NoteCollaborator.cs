using LightNotes.Domain.Enums;

namespace LightNotes.Domain.Entities;

/// <summary>
/// Учасник нотатки
/// </summary>
public class NoteCollaborator : BaseEntity
{
    public Guid NoteId { get; set; }
    public Guid UserId { get; set; }
    public Role Role { get; set; }

    public Note Note { get; set; } = null!;
    public User User { get; set; } = null!;
}
