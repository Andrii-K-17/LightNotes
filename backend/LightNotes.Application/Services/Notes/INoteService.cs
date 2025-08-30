using LightNotes.Application.DTOs.Notes;
using LightNotes.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightNotes.Application.Services.Notes;

/// <summary>
/// Сервіс для керування нотатками та їх учасниками
/// </summary>
public interface INoteService
{
    Task<List<NoteResponseDto>> GetAllNotesAsync(Guid userId); // Отримати всі нотатки користувача
    Task<NoteResponseDto?> GetNoteByIdAsync(Guid noteId, Guid userId); // Отримати нотатку за ID з правами доступу
    Task<NoteResponseDto> CreateNoteAsync(NoteRequestDto request, Guid ownerId); // Створити нотатку
    Task<NoteResponseDto?> UpdateNoteAsync(Guid noteId, NoteRequestDto request, Guid userId); // Оновити нотатку за правами
    Task<bool> DeleteNotePermanentlyAsync(Guid noteId, Guid userId); // Видалити нотатку
    Task<NoteResponseDto?> ArchiveNoteAsync(Guid noteId, Guid userId); // Архівувати нотатку
    Task<NoteResponseDto?> RestoreNoteAsync(Guid noteId, Guid userId); // Відновити нотатку з архіву
    Task<NoteCollaboratorDto?> AddCollaboratorAsync(Guid noteId, AddCollaboratorRequestDto request, Guid requestingUserId); // Додати учасника
    Task<NoteCollaboratorDto?> UpdateCollaboratorRoleAsync(Guid noteId, Guid collaboratorUserId, UpdateCollaboratorRoleRequestDto request, Guid requestingUserId); // Оновити роль учасника
    Task<bool> RemoveCollaboratorAsync(Guid noteId, Guid collaboratorUserId, Guid requestingUserId); // Видалити учасника
    Task<List<NoteCollaboratorDto>?> GetNoteCollaboratorsAsync(Guid noteId, Guid requestingUserId); // Отримати учасників нотатки
}
