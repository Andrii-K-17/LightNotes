using LightNotes.Application.DTOs.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightNotes.Application.Services.Chat;

/// <summary>
/// Сервіс для роботи з чатами нотаток
/// </summary>
public interface IChatService
{
    // Зберігає повідомлення в чаті нотатки
    Task<ChatMessageDto> SaveMessageAsync(Guid noteId, Guid senderId, SendMessageRequestDto request);

    // Отримує історію повідомлень за нотаткою
    Task<List<ChatMessageDto>?> GetChatHistoryAsync(Guid noteId, Guid userId);

    // Видаляє повідомлення в чаті нотатки
    Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);
}