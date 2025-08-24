using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;
using LightNotes.Infrastructure.Data;
using LightNotes.Infrastructure.Services.Users;
using LightNotes.Domain.Entities;
using Xunit;

namespace LightNotes.Tests.Unit.Services;

public class UserServiceTests
{
    private readonly Mock<ILogger<UserService>> _mockLogger = new();

    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
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

    private static async Task<Note> CreateTestNoteAsync(ApplicationDbContext context, Guid ownerId)
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Test note",
            OwnerId = ownerId
        };
        context.Notes.Add(note);
        await context.SaveChangesAsync();
        return note;
    }

    private static async Task<NoteCollaborator> AddTestCollaborationAsync(ApplicationDbContext context, Guid noteId, Guid userId)
    {
        var collab = new NoteCollaborator
        {
            NoteId = noteId,
            UserId = userId,
            Role = Domain.Enums.Role.Viewer
        };
        context.NoteCollaborators.Add(collab);
        await context.SaveChangesAsync();
        return collab;
    }

    [Fact]
    public async Task DeleteUserAccountAsync_ReturnsFalse_WhenUserNotFound()
    {
        await using var context = new ApplicationDbContext(CreateOptions());
        var service = new UserService(context, _mockLogger.Object);

        var result = await service.DeleteUserAccountAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteUserAccountAsync_RemovesOwnedNotes_WhenUserHasNotes()
    {
        await using var context = new ApplicationDbContext(CreateOptions());
        var user = await CreateTestUserAsync(context);
        var note = await CreateTestNoteAsync(context, user.Id);

        var service = new UserService(context, _mockLogger.Object);
        var result = await service.DeleteUserAccountAsync(user.Id);

        Assert.True(result);

        var noteInDb = await context.Notes.FindAsync(note.Id);
        Assert.Null(noteInDb);
    }

    [Fact]
    public async Task DeleteUserAccountAsync_RemovesCollaborations_WhenUserIsCollaborator()
    {
        await using var context = new ApplicationDbContext(CreateOptions());
        var owner = await CreateTestUserAsync(context, "owner@example.com");
        var collaborator = await CreateTestUserAsync(context, "collab@example.com");
        var note = await CreateTestNoteAsync(context, owner.Id);
        var collab = await AddTestCollaborationAsync(context, note.Id, collaborator.Id);

        var service = new UserService(context, _mockLogger.Object);
        var result = await service.DeleteUserAccountAsync(collaborator.Id);

        Assert.True(result);

        var collabInDb = await context.NoteCollaborators
                                      .FirstOrDefaultAsync(c => c.NoteId == note.Id && c.UserId == collaborator.Id);

        Assert.Null(collabInDb);

        var userInDb = await context.Users.FindAsync(collaborator.Id);
        Assert.Null(userInDb);
    }

    [Fact]
    public async Task DeleteUserAccountAsync_RemovesUserAndAllRelations()
    {
        await using var context = new ApplicationDbContext(CreateOptions());
        var user = await CreateTestUserAsync(context);
        var ownedNote = await CreateTestNoteAsync(context, user.Id);
        var collabNote = await CreateTestNoteAsync(context, Guid.NewGuid());

        await AddTestCollaborationAsync(context, collabNote.Id, user.Id);

        var service = new UserService(context, _mockLogger.Object);
        var result = await service.DeleteUserAccountAsync(user.Id);

        Assert.True(result);
        Assert.Null(await context.Users.FindAsync(user.Id));
        Assert.Null(await context.Notes.FindAsync(ownedNote.Id));
        Assert.Empty(await context.NoteCollaborators.Where(c => c.UserId == user.Id).ToListAsync());
    }
}
