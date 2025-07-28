using System.ComponentModel.DataAnnotations;
using LightNotes.Domain.Enums;

namespace LightNotes.Application.DTOs.Notes;

/// <summary>
/// DTO для оновлення ролі учасника нотатки
/// </summary>
public class UpdateCollaboratorRoleRequestDto
{
    [Required]
    [EnumDataType(typeof(Role))]
    public Role NewRole { get; set; }
}
