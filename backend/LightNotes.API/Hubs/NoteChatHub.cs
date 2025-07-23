using LightNotes.Application.DTOs.Chat;
using LightNotes.Application.Services.Chat;
using LightNotes.Application.Services.Notes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LightNotes.API.Hubs;

[Authorize]
public class NoteChatHub(IChatService chatService, INoteService noteService) : Hub
{
    private readonly IChatService _chatService = chatService;
    private readonly INoteService _noteService = noteService;

    /// <summary>
    /// Отримує ідентифікатор користувача з JWT.
    /// </summary>
    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            throw new HubException("Не вдалося отримати ідентифікатор користувача.");

        return userId;
    }

    /// <summary>
    /// Приєднує користувача до групи чату нотатки.
    /// </summary>
    public async Task JoinNoteChat(Guid noteId)
    {
        var userId = GetUserId();
        var note = await _noteService.GetNoteByIdAsync(noteId, userId);
        if (note == null)
            throw new HubException("У вас немає доступу до цієї замітки.");

        await Groups.AddToGroupAsync(Context.ConnectionId, noteId.ToString());
    }

    /// <summary>
    /// Виводить користувача з групи чату нотатки.
    /// </summary>
    public async Task LeaveNoteChat(Guid noteId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, noteId.ToString());
    }

    /// <summary>
    /// Відправляє повідомлення в чат нотатки.
    /// <para>Client method: <c>ReceiveMessage</c></para>
    /// </summary>
    public async Task SendMessage(Guid noteId, SendMessageRequestDto request)
    {
        var senderId = GetUserId();
        var note = await _noteService.GetNoteByIdAsync(noteId, senderId);
        if (note == null)
            throw new HubException("У вас немає доступу до цієї замітки.");

        var message = await _chatService.SaveMessageAsync(noteId, senderId, request);
        await Clients.Group(noteId.ToString()).SendAsync("ReceiveMessage", message);
    }

    /// <summary>
    /// Видаляє повідомлення з чату нотатки.
    /// <para>Client method: <c>MessageDeleted</c></para>
    /// </summary>
    public async Task DeleteMessage(Guid noteId, Guid messageId)
    {
        var userId = GetUserId();

        var result = await _chatService.DeleteMessageAsync(messageId, userId);
        if (!result)
            throw new HubException("Не вдалося видалити повідомлення.");

        await Clients.Group(noteId.ToString()).SendAsync("MessageDeleted", messageId);
    }

    /// <summary>
    /// Надсилає клієнту історію чату нотатки.
    /// <para>Client method: <c>ReceiveChatHistory</c></para>
    /// </summary>
    public async Task GetChatHistory(Guid noteId)
    {
        var userId = GetUserId();
        var history = await _chatService.GetChatHistoryAsync(noteId, userId);
        if (history == null)
            throw new HubException("У вас немає доступу до історії чату цієї замітки.");

        await Clients.Caller.SendAsync("ReceiveChatHistory", history);
    }
}
