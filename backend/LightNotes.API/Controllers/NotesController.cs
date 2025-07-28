using LightNotes.Application.DTOs.Notes;
using LightNotes.Application.Services.Notes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using LightNotes.API.Hubs;
using System.Security.Authentication;

namespace LightNotes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController(INoteService noteService, IHubContext<NoteChatHub> hubContext) : ControllerBase
{
    private readonly INoteService _noteService = noteService;
    private readonly IHubContext<NoteChatHub> _hubContext = hubContext;

    /// <summary>
    /// Отримує ідентифікатор користувача з JWT.
    /// </summary>
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new AuthenticationException("Не вдалося отримати ідентифікатор користувача з токена.");
        }

        return userId;
    }

    /// <summary>
    /// Отримати всі нотатки користувача.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllNotes()
    {
        var userId = GetUserId();
        var notes = await _noteService.GetAllNotesAsync(userId);
        return Ok(notes);
    }

    /// <summary>
    /// Отримати нотатку за її ідентифікатором.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNoteById(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { message = "Недійсний ідентифікатор." });
        }

        var userId = GetUserId();
        var note = await _noteService.GetNoteByIdAsync(id, userId);

        if (note == null)
        {
            return NotFound(new { message = "Нотатку не знайдено або у вас немає доступу." });
        }

        return Ok(note);
    }

    /// <summary>
    /// Створити нову нотатку.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateNote([FromBody] NoteRequestDto request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Дані не надано." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        var newNote = await _noteService.CreateNoteAsync(request, userId);
        return CreatedAtAction(nameof(GetNoteById), new { id = newNote.Id }, newNote);
    }

    /// <summary>
    /// Оновити існуючу нотатку.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(Guid id, [FromBody] NoteRequestDto request)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { message = "Недійсний ідентифікатор." });
        }

        if (request == null)
        {
            return BadRequest(new { message = "Дані не надано." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        var updatedNote = await _noteService.UpdateNoteAsync(id, request, userId);

        if (updatedNote == null)
        {
            return NotFound(new { message = "Нотатку не знайдено або у вас немає прав на її оновлення." });
        }

        await _hubContext.Clients.Group(id.ToString()).SendAsync("NoteUpdated", $"Нотатку '{updatedNote.Title}' оновлено користувачем {User.Identity?.Name ?? "Невідомий"}!");

        return Ok(updatedNote);
    }

    /// <summary>
    /// Архівувати нотатку.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> ArchiveNote(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { message = "Недійсний ідентифікатор." });
        }

        var userId = GetUserId();
        var note = await _noteService.ArchiveNoteAsync(id, userId);

        if (note == null)
        {
            return NotFound(new { message = "Нотатку не знайдено або у вас немає прав на архівування." });
        }

        await _hubContext.Clients.Group(id.ToString()).SendAsync("NoteArchived", $"Нотатку '{note.Title}' архівовано користувачем {User.Identity?.Name ?? "Невідомий"}!");

        return NoContent();
    }

    /// <summary>
    /// Відновити архівовану нотатку.
    /// </summary>
    [HttpPost("{id}/restore")]
    public async Task<IActionResult> RestoreNote(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { message = "Недійсний ідентифікатор." });
        }

        var userId = GetUserId();
        var note = await _noteService.RestoreNoteAsync(id, userId);

        if (note == null)
        {
            return NotFound(new { message = "Нотатку не знайдено або у вас немає прав на відновлення." });
        }

        await _hubContext.Clients.Group(id.ToString()).SendAsync("NoteRestored", $"Нотатку '{note.Title}' відновлено користувачем {User.Identity?.Name ?? "Невідомий"}!");

        return NoContent();
    }

    /// <summary>
    /// Повністю видалити нотатку.
    /// </summary>
    [HttpDelete("{id}/permanent")]
    public async Task<IActionResult> DeleteNotePermanently(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest(new { message = "Недійсний ідентифікатор." });
        }

        var userId = GetUserId();
        var result = await _noteService.DeleteNotePermanentlyAsync(id, userId);

        if (!result)
        {
            return NotFound(new { message = "Нотатку не знайдено або у вас немає прав на її повне видалення." });
        }

        await _hubContext.Clients.Group(id.ToString()).SendAsync("NoteDeleted", $"Нотатку ID {id} остаточно видалено користувачем {User.Identity?.Name ?? "Невідомий"}!");

        return NoContent();
    }

    /// <summary>
    /// Додати учасника до нотатки.
    /// </summary>
    [HttpPost("{noteId}/collaborators")]
    public async Task<IActionResult> AddCollaborator(Guid noteId, [FromBody] AddCollaboratorRequestDto request)
    {
        if (noteId == Guid.Empty)
        {
            return BadRequest(new { message = "Недійсний ідентифікатор." });
        }

        if (request == null)
        {
            return BadRequest(new { message = "Дані не надано." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var requestingUserId = GetUserId();
        var newCollaborator = await _noteService.AddCollaboratorAsync(noteId, request, requestingUserId);

        if (newCollaborator == null)
        {
            var note = await _noteService.GetNoteByIdAsync(noteId, requestingUserId);

            if (note == null)
            {
                return NotFound(new { message = "Нотатку не знайдено." });
            }

            return BadRequest(new { message = "Не вдалося додати учасника. Перевірте права або дані користувача." });
        }

        return Ok(newCollaborator);
    }

    /// <summary>
    /// Оновити роль учасника.
    /// </summary>
    [HttpPut("{noteId}/collaborators/{collaboratorUserId}")]
    public async Task<IActionResult> UpdateCollaboratorRole(Guid noteId, Guid collaboratorUserId, [FromBody] UpdateCollaboratorRoleRequestDto request)
    {
        if (noteId == Guid.Empty || collaboratorUserId == Guid.Empty)
        {
            return BadRequest(new { message = "Недійсний ідентифікатор." });
        }

        if (request == null)
        {
            return BadRequest(new { message = "Дані не надано." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var requestingUserId = GetUserId();
        var updatedCollaborator = await _noteService.UpdateCollaboratorRoleAsync(noteId, collaboratorUserId, request, requestingUserId);

        if (updatedCollaborator == null)
        {
            var note = await _noteService.GetNoteByIdAsync(noteId, requestingUserId);

            if (note == null)
            {
                return NotFound(new { message = "Нотатку не знайдено." });
            }

            return BadRequest(new { message = "Не вдалося оновити роль учасника. Перевірте права або дані." });
        }

        return Ok(updatedCollaborator);
    }

    /// <summary>
    /// Видалити учасника із нотатки.
    /// </summary>
    [HttpDelete("{noteId}/collaborators/{collaboratorUserId}")]
    public async Task<IActionResult> RemoveCollaborator(Guid noteId, Guid collaboratorUserId)
    {
        if (noteId == Guid.Empty || collaboratorUserId == Guid.Empty)
        {
            return BadRequest(new { message = "Недійсний ідентифікатор." });
        }

        var requestingUserId = GetUserId();
        var result = await _noteService.RemoveCollaboratorAsync(noteId, collaboratorUserId, requestingUserId);

        if (!result)
        {
            var note = await _noteService.GetNoteByIdAsync(noteId, requestingUserId);

            if (note == null)
            {
                return NotFound(new { message = "Нотатку не знайдено." });
            }

            return BadRequest(new { message = "Не вдалося видалити учасника. Перевірте права або дані." });
        }

        return NoContent();
    }

    /// <summary>
    /// Отримати список учасників нотатки.
    /// </summary>
    [HttpGet("{noteId}/collaborators")]
    public async Task<IActionResult> GetNoteCollaborators(Guid noteId)
    {
        if (noteId == Guid.Empty)
        {
            return BadRequest(new { message = "Недійсний ідентифікатор." });
        }

        var requestingUserId = GetUserId();
        var collaborators = await _noteService.GetNoteCollaboratorsAsync(noteId, requestingUserId);

        if (collaborators == null)
        {
            return NotFound(new { message = "Нотатку не знайдено або у вас немає доступу до її учасників." });
        }

        return Ok(collaborators);
    }
}
