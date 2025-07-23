using System;
using System.Collections.Generic;

namespace LightNotes.Application.DTOs.Notes;

/// <summary>
/// DTO нотатки, яка повертається клієнту
/// </summary>
public class NoteResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Color { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ReminderAt { get; set; }

    public List<NoteTagDto> Tags { get; set; } = [];
    public List<NoteCollaboratorDto> Collaborators { get; set; } = [];
}
