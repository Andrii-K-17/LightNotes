using LightNotes.Domain.Enums;
using System;

namespace LightNotes.Application.DTOs.Notes;

/// <summary>
/// Дані про учасника нотатки
/// </summary>
public class NoteCollaboratorDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public Role Role { get; set; }
}
