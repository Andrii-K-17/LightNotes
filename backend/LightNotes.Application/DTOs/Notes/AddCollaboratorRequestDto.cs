using LightNotes.Domain.Enums;

namespace LightNotes.Application.DTOs.Notes;

/// <summary>
/// Дані для додавання нового учасника до нотатки
/// </summary>
public class AddCollaboratorRequestDto
{
    public string UserEmail { get; set; } = string.Empty;
    public Role Role { get; set; }
}
