using AutoMapper;
using LightNotes.Application.DTOs.Chat;
using LightNotes.Application.Services.Chat;
using LightNotes.Domain.Entities;
using LightNotes.Domain.Enums;
using LightNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightNotes.Infrastructure.Services.Chat;

/// <summary>
/// Сервіс для роботи з чатами нотаток: збереження повідомлень і отримання історії
/// </summary>
/// <param name="context">Контекст бази даних</param>
/// <param name="mapper">Автоматичне відображення сутностей (AutoMapper)</param>
/// <param name="logger">Сервіс логування</param>
public class ChatService(ApplicationDbContext context, IMapper mapper, ILogger<ChatService> logger) : IChatService
{
    // Змінні для доступу до БД, мапінгу об'єктів і логування
    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<ChatService> _logger = logger;

    // Приватний метод для отримання ролі користувача в конкретній нотатці
    private async Task<Role?> GetUserNoteRoleAsync(Guid noteId, Guid userId)
    {
        // Перевіряємо, чи є користувач власником нотатки
        bool isOwner = await _context.Notes
                                 .AsNoTracking()
                                 .AnyAsync(n => n.Id == noteId && n.OwnerId == userId);

        if (isOwner)
        {
            return Role.Owner;
        }

        // Якщо не власник, перевіряємо, чи є він учасником
        var collaborator = await _context.NoteCollaborators
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(nc => nc.NoteId == noteId && nc.UserId == userId);

        if (collaborator != null)
        {
            return collaborator.Role;
        }

        // Якщо користувач не має жодної ролі у нотатці — повертаємо null
        return null;
    }

    // Метод для збереження нового повідомлення в чаті нотатки
    // Перевіряє наявність нотатки і відправника, логування і обробка помилок
    public async Task<ChatMessageDto> SaveMessageAsync(Guid noteId, Guid senderId, SendMessageRequestDto request)
    {
        _logger.LogInformation("Спроба зберегти повідомлення для нотатки ID: {NoteId} від відправника ID: {SenderId}", noteId, senderId);

        // Перевіряємо, що нотатка існує
        bool noteExists = await _context.Notes.AnyAsync(n => n.Id == noteId);
        if (!noteExists)
        {
            _logger.LogWarning("Нотатку ID: {NoteId} не знайдено.", noteId);
            throw new ArgumentException("Нотатку не знайдено.");
        }

        // Перевіряємо, що відправник існує
        var sender = await _context.Users.FindAsync(senderId);
        if (sender == null)
        {
            _logger.LogWarning("Відправника ID: {SenderId} не знайдено.", senderId);
            throw new ArgumentException("Відправника не знайдено.");
        }

        // Створюємо нове повідомлення
        var newMessage = new ChatMessage
        {
            NoteId = noteId,
            SenderId = senderId,
            Text = request.Text,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            // Додаємо повідомлення до бази і зберігаємо зміни
            _context.ChatMessages.Add(newMessage);
            await _context.SaveChangesAsync();

            // Присвоюємо відправника до повідомлення для мапінгу
            newMessage.Sender = sender;

            _logger.LogInformation("Повідомлення від {SenderName} (ID: {SenderId}) для нотатки ID: {NoteId} успішно збережено.", sender.Name, senderId, noteId);

            // Повертаємо DTO для передачі клієнту
            return _mapper.Map<ChatMessageDto>(newMessage);
        }
        catch (Exception ex)
        {
            // Логування помилки і повторне кидання винятку
            _logger.LogError(ex, "Помилка під час збереження повідомлення для нотатки ID: {NoteId} від відправника ID: {SenderId}.", noteId, senderId);
            throw;
        }
    }

    // Метод для видалення повідомлення в чаті нотатки
    public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
    {
        var message = await _context.ChatMessages.FindAsync(messageId);
        if (message == null)
            return false;

        // Перевірка, чи є користувач автором повідомлення
        if (message.SenderId != userId)
            return false;

        _context.ChatMessages.Remove(message);
        await _context.SaveChangesAsync();
        return true;
    }


    // Метод для отримання історії повідомлень чату у нотатці
    // Перевіряє роль користувача у нонатці, і якщо доступ є — повертає список повідомлень
    public async Task<List<ChatMessageDto>?> GetChatHistoryAsync(Guid noteId, Guid userId)
    {
        _logger.LogInformation("Отримання історії чату для нотатки ID: {NoteId} користувачем ID: {UserId}", noteId, userId);

        // Отримуємо роль користувача
        var userRole = await GetUserNoteRoleAsync(noteId, userId);

        if (userRole == null)
        {
            // Якщо роль відсутня — доступ заборонено, повертаємо null
            _logger.LogWarning("Користувач ID: {UserId} не має доступу до нотатки ID: {NoteId}. Відмовлено в отриманні історії чату.", userId, noteId);
            return null;
        }

        // Отримання всіх повідомлень у нотатці з інформацією про відправника, відсортованих за часом
        var chatMessages = await _context.ChatMessages
                                         .Include(cm => cm.Sender)
                                         .Where(cm => cm.NoteId == noteId)
                                         .OrderBy(cm => cm.Timestamp)
                                         .ToListAsync();

        _logger.LogInformation("Знайдено {Count} повідомлень чату для нотатки ID: {NoteId}.", chatMessages.Count, noteId);

        // Перетворення сутності у DTO і повернення
        return _mapper.Map<List<ChatMessageDto>>(chatMessages);
    }
}
