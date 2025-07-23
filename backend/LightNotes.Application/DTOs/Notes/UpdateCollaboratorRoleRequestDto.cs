using LightNotes.Domain.Enums;

namespace LightNotes.Application.DTOs.Notes;

/// <summary>
/// DTO для оновлення ролі учасника нотатки
/// </summary>
public class UpdateCollaboratorRoleRequestDto
{
    public Role NewRole { get; set; }
}
