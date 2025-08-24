using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightNotes.Application.Services.Users;

/// <summary>
/// Сервіс для роботи з користувачами нотаток
/// </summary>
public interface IUserService
{
    // Видаляє користувача
    Task<bool> DeleteUserAccountAsync(Guid userId);
}
