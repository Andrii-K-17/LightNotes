using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using LightNotes.Infrastructure.Data;
using LightNotes.Infrastructure.Services.Chat;
using LightNotes.Application.DTOs.Chat;
using LightNotes.Domain.Entities;

namespace LightNotes.Tests.Unit.Services;

public class ChatServiceTests
{
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ILogger<ChatService>> _mockLogger = new();

    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared")
            .Options;
    }

    private static ApplicationDbContext CreateContext(DbContextOptions<ApplicationDbContext> options)
    {
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static async Task<Note> CreateTestNoteAsync(ApplicationDbContext context, Guid? ownerId = null, bool isArchived = false)
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId ?? Guid.NewGuid(),
            IsArchived = isArchived
        };
        context.Notes.Add(note);
        await context.SaveChangesAsync();
        return note;
    }

    private static async Task<User> CreateTestUserAsync(ApplicationDbContext context, string? email = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email ?? $"{Guid.NewGuid()}@example.com",
            Name = "Test User"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private static async Task<List<ChatMessage>> AddTestMessagesAsync(ApplicationDbContext context, Guid noteId, Guid senderId, params string[] texts)
    {
        var messages = texts.Select(text => new ChatMessage
        {
            Id = Guid.NewGuid(),
            NoteId = noteId,
            SenderId = senderId,
            Text = text,
            Timestamp = DateTime.UtcNow
        }).ToList();

        context.ChatMessages.AddRange(messages);
        await context.SaveChangesAsync();

        return messages;
    }

    [Fact]
    public async Task SaveMessageAsync_ReturnsMappedDto_WhenValidInput()
    {
        var options = CreateOptions();
        await using var context = CreateContext(options);

        var user = await CreateTestUserAsync(context);
        var note = await CreateTestNoteAsync(context, ownerId: user.Id);

        var request = new SendMessageRequestDto { Text = "Hello" };
        var expectedDto = new ChatMessageDto
        {
            Text = "Hello",
            SenderName = user.Name
        };

        _mockMapper.Setup(m => m.Map<ChatMessageDto>(It.IsAny<ChatMessage>()))
                   .Returns(expectedDto);

        var service = new ChatService(context, _mockMapper.Object, _mockLogger.Object);
        var result = await service.SaveMessageAsync(note.Id, user.Id, request);

        Assert.NotNull(result);
        Assert.Equal(expectedDto.Text, result.Text);
        Assert.Equal(expectedDto.SenderName, result.SenderName);
    }

    [Theory]
    [InlineData("Sender", "Відправника не знайдено.")]
    [InlineData("Note", "Нотатку не знайдено.")]
    public async Task SaveMessageAsync_ThrowsArgumentException_WhenNoteOrSenderNotFound(string missing, string expectedMessage)
    {
        var options = CreateOptions();
        await using var context = CreateContext(options);

        var user = await CreateTestUserAsync(context);
        var nonExistentNoteId = Guid.NewGuid();
        var note = await CreateTestNoteAsync(context);
        var nonExistentSenderId = Guid.NewGuid();

        var request = new SendMessageRequestDto { Text = "Test" };

        var service = new ChatService(context, _mockMapper.Object, _mockLogger.Object);

        var noteId = missing == "Sender" ? note.Id : nonExistentNoteId;
        var senderId = missing == "Sender" ? nonExistentSenderId : user.Id;
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.SaveMessageAsync(noteId, senderId, request));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task DeleteMessageAsync_ReturnsTrue_WhenMessageExistsAndUserIsSender()
    {
        var options = CreateOptions();
        await using var context = CreateContext(options);

        var user = await CreateTestUserAsync(context);
        var note = await CreateTestNoteAsync(context, ownerId: user.Id);
        var messages = await AddTestMessagesAsync(context, note.Id, user.Id, "Test");

        var message = messages.First();
        var service = new ChatService(context, _mockMapper.Object, _mockLogger.Object);
        var result = await service.DeleteMessageAsync(message.Id, user.Id);

        Assert.True(result);
        Assert.Null(await context.ChatMessages.FindAsync([message.Id], TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteMessageAsync_ReturnsFalse_WhenMessageNotFound()
    {
        var options = CreateOptions();
        await using var context = CreateContext(options);
        var service = new ChatService(context, _mockMapper.Object, _mockLogger.Object);

        var result = await service.DeleteMessageAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteMessageAsync_ReturnsFalse_WhenUserIsNotSender()
    {
        var options = CreateOptions();
        await using var context = CreateContext(options);

        var sender = await CreateTestUserAsync(context);
        var otherUser = await CreateTestUserAsync(context);
        var note = await CreateTestNoteAsync(context, ownerId: sender.Id);
        var messages = await AddTestMessagesAsync(context, note.Id, sender.Id, "Not your message");

        var message = messages.First();
        var service = new ChatService(context, _mockMapper.Object, _mockLogger.Object);
        var result = await service.DeleteMessageAsync(message.Id, otherUser.Id);

        Assert.False(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetChatHistoryAsync_ReturnsMessagesOrNull_DependsOnUserAccess(bool userHasAccess)
    {
        var options = CreateOptions();
        await using var context = CreateContext(options);

        var owner = await CreateTestUserAsync(context);
        var otherUser = await CreateTestUserAsync(context);
        var note = await CreateTestNoteAsync(context, ownerId: owner.Id);
        var messages = await AddTestMessagesAsync(context, note.Id, owner.Id, "First", "Second");

        _mockMapper.Setup(m => m.Map<List<ChatMessageDto>>(It.IsAny<List<ChatMessage>>()))
                   .Returns(
                   [
                       new() { Text = "First", SenderName = "Owner" },
                       new() { Text = "Second", SenderName = "Owner" }
                   ]);

        var service = new ChatService(context, _mockMapper.Object, _mockLogger.Object);

        var userId = userHasAccess ? owner.Id : otherUser.Id;
        var result = await service.GetChatHistoryAsync(note.Id, userId);

        if (userHasAccess)
        {
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Text == "First");
            Assert.Contains(result, m => m.Text == "Second");
        }
        else
        {
            Assert.Null(result);
        }
    }
}
