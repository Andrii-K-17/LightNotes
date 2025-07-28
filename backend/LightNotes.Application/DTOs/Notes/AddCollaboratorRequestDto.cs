using System.ComponentModel.DataAnnotations;
using LightNotes.Domain.Enums;

namespace LightNotes.Application.DTOs.Notes;

/// <summary>
/// Дані для додавання нового учасника до нотатки
/// </summary>
public class AddCollaboratorRequestDto
{
    [Required]
    [EmailAddress]
    public string UserEmail { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(Role))]
    public Role Role { get; set; }
}
