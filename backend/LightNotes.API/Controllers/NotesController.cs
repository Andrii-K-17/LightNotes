using LightNotes.Application.DTOs.Notes;
using LightNotes.Application.Services.Notes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using LightNotes.API.Hubs;
using System.Security.Claims;
using System.Security.Authentication;
using LightNotes.API.Filters;

namespace LightNotes.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController(INoteService noteService, IHubContext<NoteChatHub> hubContext, ILogger<NotesController> logger) : ControllerBase
{
    private readonly INoteService _noteService = noteService;
    private readonly IHubContext<NoteChatHub> _hubContext = hubContext;
    private readonly ILogger<NotesController> _logger = logger;

    private Guid CurrentUserId => GetUserId();
    private string CurrentUserName => User.Identity?.Name ?? "Unknown user";

    /// <summary>
    /// Перевіряє, чи є переданий Guid недійсним (тобто рівним Guid.Empty).
    /// </summary>
    private static bool IsInvalid(Guid id) => id == Guid.Empty;

    /// <summary>
    /// Отримує ідентифікатор користувача з JWT.
    /// </summary>
    private Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id == null || !Guid.TryParse(id, out var userId))
        {
            _logger.LogWarning("Не вдалося отримати ID користувача з токена.");
            throw new AuthenticationException("Authentication error: User ID not found.");
        }
        return userId;
    }

    /// <summary>
    /// Надсилає повідомлення клієнтам, які підписані на групу з вказаним ідентифікатором нотатки.
    /// </summary>
    private async Task NotifyClientsAsync(Guid noteId, string method, string message)
    {
        await _hubContext.Clients.Group(noteId.ToString()).SendAsync(method, new
        {
            message,
            user = CurrentUserName
        });
    }

    /// <summary>
    /// Повертає помилку 400 (Bad Request), якщо передано недійсний ідентифікатор.
    /// </summary>
    private ObjectResult InvalidId(string name)
    {
        return Problem(
            title: "Invalid identifier",
            detail: $"{name} is invalid.",
            statusCode: StatusCodes.Status400BadRequest
        );
    }

    /// <summary>
    /// Повертає відповідь з кодом 404, якщо нотатку не знайдено або немає доступу до неї.
    /// </summary>
    private ObjectResult NoteNotFoundProblem()
    {
        return Problem(
            title: "Note not found",
            detail: "The note was not found or you do not have access to it.",
            statusCode: StatusCodes.Status404NotFound
        );
    }

    /// <summary>
    /// Повертає нотатку за ідентифікатором для поточного користувача або null, якщо не знайдено.
    /// </summary>
    private async Task<NoteResponseDto?> TryGetNote(Guid id)
    {
        var note = await _noteService.GetNoteByIdAsync(id, CurrentUserId);
        if (note == null)
        {
            _logger.LogWarning("Спроба доступу до недоступної або неіснуючої нотатки {NoteId} користувачем {UserId}.", id, CurrentUserId);
        }
        return note;
    }

    /// <summary>
    /// Отримати всі нотатки користувача.
    /// </summary>
    [HttpGet]
    [LogOperation("Get all notes")]
    public async Task<IActionResult> GetAllNotes()
    {
        _logger.LogInformation("Користувач {UserId} запитав всі нотатки.", CurrentUserId);
        var notes = await _noteService.GetAllNotesAsync(CurrentUserId);

        return Ok(notes);
    }

    /// <summary>
    /// Отримати нотатку за її ідентифікатором.
    /// </summary>
    [HttpGet("{id}")]
    [LogOperation("Get note by ID")]
    public async Task<IActionResult> GetNoteById(Guid id)
    {
        if (IsInvalid(id))
        {
            _logger.LogWarning("Отримано недійсний noteId від користувача {UserId}.", CurrentUserId);
            return InvalidId("noteId");
        }

        var note = await TryGetNote(id);
        if (note == null)
        {
            return NoteNotFoundProblem();
        }

        return Ok(note);
    }

    /// <summary>
    /// Створити нову нотатку.
    /// </summary>
    [HttpPost]
    [LogOperation("Create a new note")]
    [ValidateRequestBody("creating a note")]
    public async Task<IActionResult> CreateNote([FromBody] NoteRequestDto request)
    {
        var newNote = await _noteService.CreateNoteAsync(request, CurrentUserId);
        return CreatedAtAction(nameof(CreateNote), new { id = newNote.Id }, newNote);
    }

    /// <summary>
    /// Оновити існуючу нотатку.
    /// </summary>
    [HttpPut("{id}")]
    [LogOperation("Update an existing note")]
    [ValidateRequestBody("updating a note")]
    public async Task<IActionResult> UpdateNote(Guid id, [FromBody] NoteRequestDto request)
    {
        if (IsInvalid(id))
        {
            _logger.LogWarning("Користувач {UserId} передав недійсний ID для {MethodName}.", CurrentUserId, nameof(UpdateNote));
            return InvalidId("noteId");
        }

        var updatedNote = await _noteService.UpdateNoteAsync(id, request, CurrentUserId);
        if (updatedNote == null)
        {
            _logger.LogWarning("Користувач {UserId} не зміг оновити нотатку {NoteId}: доступ заборонено або не існує.", CurrentUserId, id);
            return NoteNotFoundProblem();
        }

        await NotifyClientsAsync(id, "NoteUpdated", $"Нотатку \"{updatedNote.Title}\" оновлено.");

        return Ok(updatedNote);
    }

    /// <summary>
    /// Архівувати нотатку.
    /// </summary>
    [HttpDelete("{id}")]
    [LogOperation("Archive note")]
    public async Task<IActionResult> ArchiveNote(Guid id)
    {
        if (IsInvalid(id))
        {
            _logger.LogWarning("Користувач {UserId} передав недійсний ID для {MethodName}.", CurrentUserId, nameof(ArchiveNote));
            return InvalidId("noteId");
        }

        var note = await _noteService.ArchiveNoteAsync(id, CurrentUserId);
        if (note == null)
        {
            _logger.LogWarning("Користувач {UserId} не знайшов нотатку {NoteId} для архівації.", CurrentUserId, id);
            return NoteNotFoundProblem();
        }

        await NotifyClientsAsync(id, "NoteArchived", $"Нотатку \"{note.Title}\" перенесено до архіву.");

        return NoContent();
    }

    /// <summary>
    /// Відновити архівовану нотатку.
    /// </summary>
    [HttpPost("{id}/restore")]
    [LogOperation("Restore note")]
    public async Task<IActionResult> RestoreNote(Guid id)
    {
        if (IsInvalid(id))
        {
            _logger.LogWarning("Користувач {UserId} передав недійсний ID для {MethodName}.", CurrentUserId, nameof(RestoreNote));
            return InvalidId("noteId");
        }

        var note = await _noteService.RestoreNoteAsync(id, CurrentUserId);
        if (note == null)
        {
            _logger.LogWarning("Користувач {UserId} не знайшов нотатку {NoteId} для відновлення.", CurrentUserId, id);
            return NoteNotFoundProblem();
        }

        await NotifyClientsAsync(id, "NoteRestored", $"Нотатку \"{note.Title}\" відновлено.");

        return Ok(note);
    }

    /// <summary>
    /// Повністю видалити нотатку.
    /// </summary>
    [HttpDelete("{id}/permanent")]
    [LogOperation("Permanently delete note")]
    public async Task<IActionResult> DeleteNotePermanently(Guid id)
    {
        if (IsInvalid(id))
        {
            _logger.LogWarning("Користувач {UserId} передав недійсний ID для {MethodName}.", CurrentUserId, nameof(DeleteNotePermanently));
            return InvalidId("noteId");
        }

        var result = await _noteService.DeleteNotePermanentlyAsync(id, CurrentUserId);
        if (!result)
        {
            _logger.LogWarning("Користувач {UserId} не зміг повністю видалити нотатку {NoteId}.", CurrentUserId, id);
            return Problem(
                title: "Resource not found",
                detail: "The note was not found or you don't have permission to delete it.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        await NotifyClientsAsync(id, "NoteDeleted", $"Нотатку ID {id} остаточно видалено.");

        return NoContent();
    }

    /// <summary>
    /// Додати учасника до нотатки.
    /// </summary>
    [HttpPost("{noteId}/collaborators")]
    [LogOperation("Add collaborator to note")]
    [ValidateRequestBody("adding a collaborator")]
    public async Task<IActionResult> AddCollaborator(Guid noteId, [FromBody] AddCollaboratorRequestDto request)
    {
        if (IsInvalid(noteId))
        {
            _logger.LogWarning("Користувач {UserId} передав недійсний noteId для {MethodName}.", CurrentUserId, nameof(AddCollaborator));
            return InvalidId("noteId");
        }

        var newCollaborator = await _noteService.AddCollaboratorAsync(noteId, request, CurrentUserId);
        if (newCollaborator == null)
        {
            _logger.LogWarning("Користувач {UserId} не зміг додати учасника до нотатки {NoteId}.", CurrentUserId, noteId);
            return Problem(
                title: "Failed to add collaborator",
                detail: "Please check permissions or user data.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return Ok(newCollaborator);
    }

    /// <summary>
    /// Оновити роль учасника.
    /// </summary>
    [HttpPut("{noteId}/collaborators/{collaboratorUserId}")]
    [LogOperation("Update collaborator role")]
    [ValidateRequestBody("updating a role")]
    public async Task<IActionResult> UpdateCollaboratorRole(Guid noteId, Guid collaboratorUserId, [FromBody] UpdateCollaboratorRoleRequestDto request)
    {
        if (IsInvalid(noteId) || IsInvalid(collaboratorUserId))
        {
            _logger.LogWarning("Користувач {UserId} передав недійсний noteId або collaboratorUserId для {MethodName}.", CurrentUserId, nameof(UpdateCollaboratorRole));
            return InvalidId("noteId або userId");
        }

        var updatedCollaborator = await _noteService.UpdateCollaboratorRoleAsync(noteId, collaboratorUserId, request, CurrentUserId);
        if (updatedCollaborator == null)
        {
            _logger.LogWarning("Користувач {UserId} не зміг оновити роль учасника {CollaboratorUserId} у нотатці {NoteId}.", CurrentUserId, collaboratorUserId, noteId);
            return Problem(
                title: "Failed to update collaborator role",
                detail: "Please check permissions or user data.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return Ok(updatedCollaborator);
    }

    /// <summary>
    /// Видалити учасника із нотатки.
    /// </summary>
    [HttpDelete("{noteId}/collaborators/{collaboratorUserId}")]
    [LogOperation("Remove collaborator from note")]
    public async Task<IActionResult> RemoveCollaborator(Guid noteId, Guid collaboratorUserId)
    {
        if (IsInvalid(noteId) || IsInvalid(collaboratorUserId))
        {
            _logger.LogWarning("Користувач {UserId} передав недійсний noteId або userId для {MethodName}.", CurrentUserId, nameof(RemoveCollaborator));
            return InvalidId("noteId або userId");
        }

        var result = await _noteService.RemoveCollaboratorAsync(noteId, collaboratorUserId, CurrentUserId);
        if (!result)
        {
            _logger.LogWarning("Користувач {UserId} не зміг видалити учасника {CollaboratorUserId} з нотатки {NoteId}.", CurrentUserId, collaboratorUserId, noteId);
            return Problem(
                title: "Failed to remove collaborator",
                detail: "Please check permissions or data.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return NoContent();
    }

    /// <summary>
    /// Отримати список учасників нотатки.
    /// </summary>
    [HttpGet("{noteId}/collaborators")]
    [LogOperation("Get note collaborators")]
    public async Task<IActionResult> GetNoteCollaborators(Guid noteId)
    {
        if (IsInvalid(noteId))
        {
            _logger.LogWarning("Користувач {UserId} передав недійсний noteId для {MethodName}.", CurrentUserId, nameof(GetNoteCollaborators));
            return InvalidId("noteId");
        }

        var collaborators = await _noteService.GetNoteCollaboratorsAsync(noteId, CurrentUserId);
        if (collaborators == null)
        {
            _logger.LogWarning("Користувач {UserId} не має доступу або нотатку {NoteId} не знайдено.", CurrentUserId, noteId);
            return Problem(
                title: "Note not found or access denied",
                detail: "The note was not found or you do not have access to its collaborators.",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return Ok(collaborators);
    }
}
