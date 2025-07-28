using System.ComponentModel.DataAnnotations;

namespace LightNotes.Domain.Entities;

/// <summary>
/// Тег нотатки
/// </summary>
public class NoteTag : BaseEntity
{
    public Guid NoteId { get; set; }
    
    [Required]
    [MaxLength(30)]
    public string Tag { get; set; } = string.Empty;

    public Note Note { get; set; } = null!;
}
