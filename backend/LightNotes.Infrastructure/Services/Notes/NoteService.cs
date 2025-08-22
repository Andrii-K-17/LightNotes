using AutoMapper;
using LightNotes.Application.DTOs.Notes;
using LightNotes.Application.Services.Notes;
using LightNotes.Domain.Entities;
using LightNotes.Domain.Enums;
using LightNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightNotes.Infrastructure.Services.Notes;

/// <summary>
/// Сервіс для роботи з нотатками: CRUD та керування учасниками
/// </summary>
/// <param name="context">Контекст бази даних</param>
/// <param name="mapper">Сервіс мапінгу (AutoMapper)</param>
/// <param name="logger">Сервіс логування</param>
public class NoteService(ApplicationDbContext context, IMapper mapper, ILogger<NoteService> logger) : INoteService
{
    // Приватні поля для доступу до контексту бази даних, AutoMapper та логера
    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<NoteService> _logger = logger;

    // Метод для перевірки, чи має користувач доступ до нотатки та яка його роль
    private async Task<Role?> GetUserNoteRoleAsync(Guid noteId, Guid userId)
    {
        // Чи є користувач власником нотатки
        bool isOwner = await _context.Notes
                                 .AsNoTracking() // Не відстежувати зміни для простого читання
                                 .AnyAsync(n => n.Id == noteId && n.OwnerId == userId);

        if (isOwner)
        {
            return Role.Admin;
        }

        // Якщо не власник, перевіряємо, чи є він учасником
        var collaborator = await _context.NoteCollaborators
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(nc => nc.NoteId == noteId && nc.UserId == userId);

        if (collaborator != null)
        {
            return collaborator.Role; // Користувач є учасником, повертаємо його роль
        }

        return null; // Користувач не має доступу до нотатки
    }

    // Перевірка чи є користувач власником нотатки
    private async Task<bool> IsUserNoteOwnerAsync(Guid noteId, Guid userId)
    {
        var role = await GetUserNoteRoleAsync(noteId, userId);
        return role == Role.Admin;
    }


    /// Отримує всі нотатки, до яких користувач має доступ
    public async Task<List<NoteResponseDto>> GetAllNotesAsync(Guid userId)
    {
        _logger.LogInformation("Отримання всіх нотаток для користувача ID: {UserId}", userId);
        try
        {
            // Отримуємо нотатки, де користувач є власником
            var ownedNotes = await _context.Notes
                                           .Include(n => n.Owner) // Включаємо дані власника
                                           .Include(n => n.Tags) // Включаємо теги
                                           .Include(n => n.Collaborators) // Включаємо учасників
                                                .ThenInclude(nc => nc.User) // Включаємо дані користувача для учасників
                                           .Where(n => n.OwnerId == userId)
                                           .ToListAsync();

            // Отримуємо нотатки, де користувач є учасником
            var collaboratedNotes = await _context.NoteCollaborators
                                                .Include(nc => nc.Note) // Включаємо саму нотатку
                                                    .ThenInclude(n => n.Owner) // Включаємо власника нотатки
                                                .Include(nc => nc.Note)
                                                    .ThenInclude(n => n.Tags) // Включаємо теги нотатки
                                                .Include(nc => nc.Note) // Повторно включаємо Note для доступу до його Collaborators
                                                    .ThenInclude(n => n.Collaborators) // Включаємо учасників нотатки
                                                        .ThenInclude(nc => nc.User) // Включаємо дані користувача для учасників
                                                .Where(nc => nc.UserId == userId)
                                                .Select(nc => nc.Note) // Вибираємо об'єкт Note
                                                .ToListAsync();

            // Використовуємо Union для об'єднання та DistinctBy для унікальності за Id
            var allNotes = ownedNotes
                            .Union(collaboratedNotes) // Об'єднуємо обидва списки
                            .DistinctBy(n => n.Id) // Видаляємо дублікати (якщо нотатка є і власною, і спільною)
                            .ToList();

            _logger.LogInformation("Знайдено {Count} нотаток для користувача ID: {UserId}", allNotes.Count, userId);
            // Перетворюємо сутності Note на NoteResponseDto за допомогою AutoMapper
            return _mapper.Map<List<NoteResponseDto>>(allNotes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час отримання всіх нотаток для користувача ID: {UserId}", userId);
            throw;
        }
    }

    // Отримує одну нотатку за її ідентифікатором, якщо користувач має доступ
    public async Task<NoteResponseDto?> GetNoteByIdAsync(Guid noteId, Guid userId)
    {
        _logger.LogInformation("Отримання нотатки ID: {NoteId} для користувача ID: {UserId}", noteId, userId);
        // Перевіряємо роль користувача для цієї нотатки.
        var userRole = await GetUserNoteRoleAsync(noteId, userId);
        if (userRole == null)
        {
            _logger.LogWarning("Користувач ID: {UserId} не має доступу до нотатки ID: {NoteId}", userId, noteId);
            return null; // Користувач не має доступу
        }

        // Отримуємо нотатку, включаючи власника, теги та учасників з їхніми даними
        var note = await _context.Notes
                                 .Include(n => n.Owner)
                                 .Include(n => n.Tags)
                                 .Include(n => n.Collaborators)
                                     .ThenInclude(nc => nc.User)
                                 .FirstOrDefaultAsync(n => n.Id == noteId);

        if (note == null)
        {
            _logger.LogWarning("Нотатку ID: {NoteId} не знайдено.", noteId);
            return null;
        }

        _logger.LogInformation("Нотатку ID: {NoteId} успішно отримано.", noteId);
        // Перетворюємо сутність Note на NoteResponseDto за допомогою AutoMapper
        return _mapper.Map<NoteResponseDto>(note);
    }


    // Створює нову нотатку для вказаного користувача
    public async Task<NoteResponseDto> CreateNoteAsync(NoteRequestDto request, Guid ownerId)
    {
        _logger.LogInformation("Створення нової нотатки для власника ID: {OwnerId}", ownerId);
    
        // Перетворення NoteRequestDto на сутність Note
        var newNote = _mapper.Map<Note>(request);
        newNote.OwnerId = ownerId;
    
        // Додаємо теги
        newNote.Tags = request.Tags
                              .Select(tagDto => new NoteTag { Tag = tagDto.Tag })
                              .ToList();
    
        try
        {
            _context.Notes.Add(newNote);
            await _context.SaveChangesAsync();
    
            // Повністю перезавантажуємо нотатку з усіма потрібними навігаційними властивостями одним запитом
            var fullNote = await _context.Notes
                .Include(n => n.Owner)
                .Include(n => n.Tags)
                .Include(n => n.Collaborators)
                    .ThenInclude(nc => nc.User)
                .FirstOrDefaultAsync(n => n.Id == newNote.Id);
    
            if (fullNote == null)
            {
                _logger.LogWarning("Нотатку після збереження не знайдено. ID: {NoteId}", newNote.Id);
                throw new InvalidOperationException("Не вдалося завантажити створену нотатку.");
            }
    
            _logger.LogInformation("Нотатку '{Title}' (ID: {NoteId}) успішно створено власником ID: {OwnerId}", fullNote.Title, fullNote.Id, ownerId);
            return _mapper.Map<NoteResponseDto>(fullNote);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час створення нотатки для власника ID: {OwnerId}", ownerId);
            throw;
        }
    }


    //Оновлює існуючу нотатку
    public async Task<NoteResponseDto?> UpdateNoteAsync(Guid noteId, NoteRequestDto request, Guid userId)
    {
        _logger.LogInformation("Спроба оновлення нотатки ID: {NoteId} користувачем ID: {UserId}", noteId, userId);
        // Перевіряємо роль користувача. Дозволено оновлювати лише Власнику або Редактору
        var userRole = await GetUserNoteRoleAsync(noteId, userId);
        if (userRole == null || (userRole != Role.Admin && userRole != Role.Editor))
        {
            _logger.LogWarning("Користувач ID: {UserId} не має прав на оновлення нотатки ID: {NoteId}. Роль: {Role}", userId, noteId, userRole);
            return null; // Користувач не має достатніх прав
        }

        // Знаходимо нотатку, яку потрібно оновити. Включаємо теги, власника та учасників
        var noteToUpdate = await _context.Notes
                                         .Include(n => n.Tags)
                                         .Include(n => n.Owner)
                                         .Include(n => n.Collaborators)
                                            .ThenInclude(nc => nc.User)
                                         .FirstOrDefaultAsync(n => n.Id == noteId);

        if (noteToUpdate == null)
        {
            _logger.LogWarning("Нотатку ID: {NoteId} не знайдено для оновлення.", noteId);
            return null; // Нотатку не знайдено
        }

        // Використовуємо AutoMapper для оновлення існуючої сутності Note з даних NoteRequestDto
        _mapper.Map(request, noteToUpdate);
        noteToUpdate.UpdatedAt = DateTime.UtcNow; // Цю властивість оновлюємо вручну

        // Оновлення тегів
        _context.NoteTags.RemoveRange(noteToUpdate.Tags); // Видаляємо старі теги
        noteToUpdate.Tags.Clear(); // Очищаємо колекцію в пам'яті

        foreach (var tagDto in request.Tags)
        {
            noteToUpdate.Tags.Add(new NoteTag { NoteId = noteToUpdate.Id, Tag = tagDto.Tag }); // Додаємо нові теги
        }

        try
        {
            await _context.SaveChangesAsync(); // Зберігаємо зміни в БД
            _logger.LogInformation("Нотатку '{Title}' (ID: {NoteId}) успішно оновлено користувачем ID: {UserId}", noteToUpdate.Title, noteToUpdate.Id, userId);
            return _mapper.Map<NoteResponseDto>(noteToUpdate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час оновлення нотатки ID: {NoteId} користувачем ID: {UserId}", noteId, userId);
            throw;
        }
    }

    // Архівування нотатки
    public async Task<Note?> ArchiveNoteAsync(Guid noteId, Guid userId)
    {
        _logger.LogInformation("Спроба архівування нотатки ID: {NoteId} користувачем ID: {UserId}", noteId, userId);

        if (!await IsUserNoteOwnerAsync(noteId, userId))
        {
            _logger.LogWarning("Користувач ID: {UserId} не має прав на архівування нотатки ID: {NoteId}", userId, noteId);
            return null;
        }

        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == noteId);
        if (note == null)
        {
            _logger.LogWarning("Нотатку ID: {NoteId} не знайдено для архівування.", noteId);
            return null;
        }

        if (note.IsArchived)
        {
            _logger.LogInformation("Нотатку ID: {NoteId} вже архівовано.", noteId);
            return note; // Повертаємо існуючу архівовану нотатку
        }

        note.IsArchived = true;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Нотатку '{Title}' (ID: {NoteId}) успішно архівовано користувачем ID: {UserId}", note.Title, note.Id, userId);
            return note;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час архівування нотатки ID: {NoteId} користувачем ID: {UserId}", noteId, userId);
            throw;
        }
    }

    // Відновлення архівованої нотатки
    public async Task<Note?> RestoreNoteAsync(Guid noteId, Guid userId)
    {
        _logger.LogInformation("Спроба відновлення нотатки ID: {NoteId} користувачем ID: {UserId}", noteId, userId);

        if (!await IsUserNoteOwnerAsync(noteId, userId))
        {
            _logger.LogWarning("Користувач ID: {UserId} не має прав на відновлення нотатки ID: {NoteId}", userId, noteId);
            return null;
        }

        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == noteId);
        if (note == null)
        {
            _logger.LogWarning("Нотатку ID: {NoteId} не знайдено для відновлення.", noteId);
            return null;
        }

        if (!note.IsArchived)
        {
            _logger.LogInformation("Нотатку ID: {NoteId} не потрібно відновлювати - вона не архівована.", noteId);
            return note;
        }

        note.IsArchived = false;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Нотатку '{Title}' (ID: {NoteId}) успішно відновлено користувачем ID: {UserId}", note.Title, note.Id, userId);
            return note;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час відновлення нотатки ID: {NoteId} користувачем ID: {UserId}", noteId, userId);
            throw;
        }
    }

    // Повне видалення нотатки (hard delete)
    public async Task<bool> DeleteNotePermanentlyAsync(Guid noteId, Guid userId)
    {
        _logger.LogInformation("Спроба повного видалення нотатки ID: {NoteId} користувачем ID: {UserId}", noteId, userId);

        var userRole = await GetUserNoteRoleAsync(noteId, userId);
        if (userRole != Role.Admin)
        {
            _logger.LogWarning("Користувач не має прав на повне видалення нотатки ID: {NoteId}. Роль: {Role}", noteId, userRole);
            return false;
        }

        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == noteId);
        if (note == null)
        {
            _logger.LogWarning("Нотатку не знайдено для повного видалення. ID: {NoteId}", noteId);
            return false;
        }

        _context.Notes.Remove(note);

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Нотатку ID: {NoteId} повністю видалено", noteId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при повному видаленні нотатки ID: {NoteId}", noteId);
            throw;
        }
    }

    // Додає нового учасника до нотатки
    public async Task<NoteCollaboratorDto?> AddCollaboratorAsync(Guid noteId, AddCollaboratorRequestDto request, Guid requestingUserId)
    {
        _logger.LogInformation("Спроба додати учасника {UserEmail} до нотатки ID: {NoteId} користувачем ID: {RequestingUserId}", request.UserEmail, noteId, requestingUserId);

        // Завантаження нотатки з власником
        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == noteId);
        if (note == null)
        {
            _logger.LogWarning("Нотатку ID: {NoteId} не знайдено.", noteId);
            return null;
        }

        // Перевірка прав доступу
        if (note.OwnerId != requestingUserId)
        {
            _logger.LogWarning("Користувач ID: {RequestingUserId} не є власником нотатки ID: {NoteId}.", requestingUserId, noteId);
            return null;
        }

        // Знаходження користувача, якого потрібно додати
        var userToAdd = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.UserEmail);
        if (userToAdd == null)
        {
            _logger.LogWarning("Користувача з Email: {UserEmail} не знайдено.", request.UserEmail);
            return null;
        }

        // Перевірки дублювання або власника
        if (note.OwnerId == userToAdd.Id)
        {
            _logger.LogWarning("Користувач {UserEmail} вже є власником нотатки ID: {NoteId}.", request.UserEmail, noteId);
            return null;
        }

        bool isAlreadyCollaborator = await _context.NoteCollaborators
                                                   .AnyAsync(nc => nc.NoteId == noteId && nc.UserId == userToAdd.Id);

        if (isAlreadyCollaborator)
        {
            _logger.LogWarning("Користувач {UserEmail} вже є учасником нотатки ID: {NoteId}.", request.UserEmail, noteId);
            return null;
        }

        // Заборона призначення ролі Owner
        if (request.Role == Role.Admin)
        {
            _logger.LogWarning("Спроба призначити роль Owner через додавання учасника до нотатки ID: {NoteId}.", noteId);
            return null;
        }

        // Додавання учасника
        var newCollaborator = new NoteCollaborator
        {
            NoteId = noteId,
            UserId = userToAdd.Id,
            Role = request.Role,
            User = userToAdd // для мапінгу DTO
        };

        _context.NoteCollaborators.Add(newCollaborator);

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Учасника {UserEmail} успішно додано до нотатки ID: {NoteId} з роллю {Role}.", request.UserEmail, noteId, request.Role);
            return _mapper.Map<NoteCollaboratorDto>(newCollaborator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час додавання учасника {UserEmail} до нотатки ID: {NoteId}.", request.UserEmail, noteId);
            throw;
        }
    }

    // Оновлює роль існуючого учасника нотатки
    public async Task<NoteCollaboratorDto?> UpdateCollaboratorRoleAsync(Guid noteId, Guid collaboratorUserId, UpdateCollaboratorRoleRequestDto request, Guid requestingUserId)
    {
        _logger.LogInformation("Спроба оновити роль учасника ID: {CollaboratorUserId} для нотатки ID: {NoteId} користувачем ID: {RequestingUserId}", collaboratorUserId, noteId, requestingUserId);

        // Тільки власник нотатки може змінювати ролі
        if (!await IsUserNoteOwnerAsync(noteId, requestingUserId))
        {
            _logger.LogWarning("Користувач ID: {RequestingUserId} не є власником нотатки ID: {NoteId}. Відмовлено в оновленні ролі учасника.", requestingUserId, noteId);
            return null;
        }

        // Перевірка, чи користувач, чию роль змінюємо, не є власником нотатки (власника не можна змінити)
        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == noteId);
        if (note == null || note.OwnerId == collaboratorUserId)
        {
            _logger.LogWarning("Нотатку ID: {NoteId} не знайдено або спроба змінити роль власника ID: {CollaboratorUserId}.", noteId, collaboratorUserId);
            return null;
        }

        // Знайти існуючого учасника
        var collaboratorToUpdate = await _context.NoteCollaborators
                                                 .Include(nc => nc.User) // Включаємо дані користувача
                                                 .FirstOrDefaultAsync(nc => nc.NoteId == noteId && nc.UserId == collaboratorUserId);
        if (collaboratorToUpdate == null)
        {
            _logger.LogWarning("Учасника ID: {CollaboratorUserId} не знайдено для нотатки ID: {NoteId}.", collaboratorUserId, noteId);
            return null;
        }

        // Перевірка, чи нова роль є дійсною (не можна призначити роль Owner через цей метод)
        if (request.NewRole == Role.Admin)
        {
            _logger.LogWarning("Спроба призначити роль Owner через оновлення ролі для учасника ID: {CollaboratorUserId} в нотатці ID: {NoteId}.", collaboratorUserId, noteId);
            return null;
        }

        // Оновлення ролі
        collaboratorToUpdate.Role = request.NewRole;

        try
        {
            _context.NoteCollaborators.Update(collaboratorToUpdate);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Роль учасника ID: {CollaboratorUserId} для нотатки ID: {NoteId} успішно оновлено до {NewRole}.", collaboratorUserId, noteId, request.NewRole);
            return _mapper.Map<NoteCollaboratorDto>(collaboratorToUpdate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час оновлення ролі учасника ID: {CollaboratorUserId} для нотатки ID: {NoteId}.", collaboratorUserId, noteId);
            throw;
        }
    }

    // Видаляє учасника із нотатки
    public async Task<bool> RemoveCollaboratorAsync(Guid noteId, Guid collaboratorUserId, Guid requestingUserId)
    {
        _logger.LogInformation("Спроба видалити учасника ID: {CollaboratorUserId} із нотатки ID: {NoteId} користувачем ID: {RequestingUserId}", collaboratorUserId, noteId, requestingUserId);

        // Тільки власник нотатки може видаляти учасників
        if (!await IsUserNoteOwnerAsync(noteId, requestingUserId))
        {
            _logger.LogWarning("Користувач ID: {RequestingUserId} не є власником нотатки ID: {NoteId}. Відмовлено у видаленні учасника.", requestingUserId, noteId);
            return false;
        }

        // Перевірка, чи користувач, якого видаляємо, не є власником нотатки
        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == noteId);
        if (note == null || note.OwnerId == collaboratorUserId)
        {
            _logger.LogWarning("Нотатку ID: {NoteId} не знайдено або спроба видалити власника ID: {CollaboratorUserId}.", noteId, collaboratorUserId);
            return false;
        }

        // Знайти існуючого учасника
        var collaboratorToRemove = await _context.NoteCollaborators
                                                 .FirstOrDefaultAsync(nc => nc.NoteId == noteId && nc.UserId == collaboratorUserId);
        if (collaboratorToRemove == null)
        {
            _logger.LogWarning("Учасника ID: {CollaboratorUserId} не знайдено для нотатки ID: {NoteId}.", collaboratorUserId, noteId);
            return false;
        }

        try
        {
            _context.NoteCollaborators.Remove(collaboratorToRemove);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Учасника ID: {CollaboratorUserId} успішно видалено із нотатки ID: {NoteId}.", collaboratorUserId, noteId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час видалення учасника ID: {CollaboratorUserId} із нотатки ID: {NoteId}.", collaboratorUserId, noteId);
            throw;
        }
    }

    // Отримує список усіх учасників для певної нотатки
    public async Task<List<NoteCollaboratorDto>?> GetNoteCollaboratorsAsync(Guid noteId, Guid requestingUserId)
    {
        _logger.LogInformation("Отримання учасників для нотатки ID: {NoteId} користувачем ID: {RequestingUserId}", noteId, requestingUserId);

        // Перевірка прав: користувач повинен бути власником або учасником нотатки, щоб бачити список учасників
        var requestingUserRole = await GetUserNoteRoleAsync(noteId, requestingUserId);
        if (requestingUserRole == null)
        {
            _logger.LogWarning("Користувач ID: {RequestingUserId} не має доступу до нотатки ID: {NoteId}. Відмовлено в отриманні учасників.", requestingUserId, noteId);
            return null;
        }

        // Отримуємо всіх учасників для цієї нотатки, включаючи їхні дані User
        var collaborators = await _context.NoteCollaborators
                                          .Include(nc => nc.User) // Важливо включити дані користувача для мапінгу DTO
                                          .Where(nc => nc.NoteId == noteId)
                                          .ToListAsync();

        _logger.LogInformation("Знайдено {Count} учасників для нотатки ID: {NoteId}.", collaborators.Count, noteId);
        return _mapper.Map<List<NoteCollaboratorDto>>(collaborators);
    }
}
