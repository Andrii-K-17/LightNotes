using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LightNotes.Application.DTOs.Notes;

/// <summary>
/// DTO, що передається при створенні чи оновленні нотатки
/// </summary>
public class NoteRequestDto
{
    [Required]
    [MaxLength(100, ErrorMessage = "Заголовок не повинен перевищувати 100 символів.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "Колір не повинен перевищувати 20 символів.")]
    public string? Color { get; set; }

    public bool IsPinned { get; set; }
    public bool IsArchived { get; set; }

    public DateTime? ReminderAt { get; set; }

    public List<NoteTagDto> Tags { get; set; } = new();
}
