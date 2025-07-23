using System;
using System.Collections.Generic;

namespace LightNotes.Application.DTOs.Notes;

/// <summary>
/// DTO, що передається при створенні чи оновленні нотатки
/// </summary>
public class NoteRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Color { get; set; }
    public bool IsPinned { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ReminderAt { get; set; }
    public List<NoteTagDto> Tags { get; set; } = [];
}
