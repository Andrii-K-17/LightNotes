using LightNotes.Application.Services.Users;
using LightNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightNotes.Infrastructure.Services.Users;

/// <summary>
/// Сервіс для роботи з користувачем
/// </summary>
/// <param name="context">Контекст бази даних</param>
/// <param name="logger">Сервіс логування</param>
public class UserService(ApplicationDbContext context, ILogger<UserService> logger) : IUserService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<UserService> _logger = logger;

    // Видаляє користувача за id
    public async Task<bool> DeleteUserAccountAsync(Guid userId)
    {
        _logger.LogInformation("Спроба видалення облікового запису користувача ID: {UserId}", userId);

        try
        {
            var userToDelete = await _context.Users
                                             .Include(u => u.OwnedNotes)
                                             .Include(u => u.Collaborations)
                                             .FirstOrDefaultAsync(u => u.Id == userId);

            if (userToDelete == null)
            {
                _logger.LogWarning("Користувача ID: {UserId} не знайдено для видалення.", userId);
                return false;
            }

            // Видаляємо всі нотатки, які належать цьому користувачу
            _context.Notes.RemoveRange(userToDelete.OwnedNotes);

            // Видаляємо всі співавторства для цього користувача
            _context.NoteCollaborators.RemoveRange(userToDelete.Collaborations);

            // Видаляємо самого користувача
            _context.Users.Remove(userToDelete);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Обліковий запис користувача ID: {UserId} успішно видалено.", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка під час видалення облікового запису користувача ID: {UserId}", userId);
            throw;
        }
    }
}
