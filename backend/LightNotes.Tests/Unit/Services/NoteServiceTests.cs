using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using LightNotes.Infrastructure.Data;
using LightNotes.Infrastructure.Services.Notes;
using LightNotes.Application.DTOs.Notes;
using LightNotes.Domain.Entities;
using LightNotes.Domain.Enums;

namespace LightNotes.Tests.Unit.Services;

public class NoteServiceTests
{
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<ILogger<NoteService>> _mockLogger = new();

    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
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

    private static async Task AddTestCollaboratorAsync(ApplicationDbContext context, Guid noteId, Guid userId, Role role = Role.Viewer)
    {
        var collaborator = new NoteCollaborator
        {
            NoteId = noteId,
            UserId = userId,
            Role = role
        };
        context.NoteCollaborators.Add(collaborator);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllNotesAsync_ReturnsMappedNotes_WhenUserIsOwnerAndCollaborator()
    {
        await using var context = new ApplicationDbContext(CreateOptions());
        var user = await CreateTestUserAsync(context);

        var ownedNote = await CreateTestNoteAsync(context, ownerId: user.Id);
        var otherNote = await CreateTestNoteAsync(context);

        await AddTestCollaboratorAsync(context, otherNote.Id, user.Id);

        _mockMapper.Setup(m => m.Map<List<NoteResponseDto>>(It.IsAny<List<Note>>()))
                   .Returns(
                   [
                       new() { Id = ownedNote.Id, Title = "Owned Note" },
                       new() { Id = otherNote.Id, Title = "Collaborated Note" }
                   ]);

        var service = new NoteService(context, _mockMapper.Object, _mockLogger.Object);
        var result = await service.GetAllNotesAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetNoteByIdAsync_ReturnsCorrectNote_WhenAccessDependsOnUser(bool userHasAccess)
    {
        await using var context = new ApplicationDbContext(CreateOptions());

        var noteOwner = await CreateTestUserAsync(context);
        var anotherUser = await CreateTestUserAsync(context);

        var testNote = await CreateTestNoteAsync(context, ownerId: noteOwner.Id);
        var currentUserId = userHasAccess ? anotherUser.Id : Guid.NewGuid();

        if (userHasAccess)
        {
            await AddTestCollaboratorAsync(context, testNote.Id, anotherUser.Id);

            _mockMapper.Setup(m => m.Map<NoteResponseDto>(It.IsAny<Note>()))
                       .Returns(new NoteResponseDto
                       {
                           Id = testNote.Id,
                           Title = "Some note title"
                       });
        }

        var service = new NoteService(context, _mockMapper.Object, _mockLogger.Object);

        var fetchedNote = await service.GetNoteByIdAsync(testNote.Id, currentUserId);

        if (userHasAccess)
        {
            Assert.NotNull(fetchedNote);
            Assert.Equal(testNote.Id, fetchedNote.Id);
        }
        else
        {
            Assert.Null(fetchedNote);
        }
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ArchiveNoteAsync_ReturnsExpectedResult_DependentOnUserOwnership(bool userIsOwner)
    {
        await using var context = new ApplicationDbContext(CreateOptions());

        var creator = await CreateTestUserAsync(context);
        var randomUser = await CreateTestUserAsync(context);

        var originalNote = await CreateTestNoteAsync(context, ownerId: creator.Id, isArchived: false);
        var callerUserId = userIsOwner ? creator.Id : randomUser.Id;

        var service = new NoteService(context, _mockMapper.Object, _mockLogger.Object);

        var archivedNote = await service.ArchiveNoteAsync(originalNote.Id, callerUserId);

        if (userIsOwner)
        {
            Assert.NotNull(archivedNote);
            Assert.True(archivedNote.IsArchived);
        }
        else
        {
            Assert.Null(archivedNote);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RestoreNoteAsync_ReturnsExpectedResult_DependingOnOwnership(bool userIsOwner)
    {
        await using var context = new ApplicationDbContext(CreateOptions());

        var owner = await CreateTestUserAsync(context);
        var nonOwner = await CreateTestUserAsync(context);

        var archivedNote = await CreateTestNoteAsync(context, ownerId: owner.Id, isArchived: true);
        var currentUserId = userIsOwner ? owner.Id : nonOwner.Id;

        var service = new NoteService(context, _mockMapper.Object, _mockLogger.Object);

        var result = await service.RestoreNoteAsync(archivedNote.Id, currentUserId);

        if (userIsOwner)
        {
            Assert.NotNull(result);
            Assert.False(result.IsArchived);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteNotePermanentlyAsync_ReturnsExpectedResult_BasedOnOwnership(bool userIsOwner)
    {
        await using var context = new ApplicationDbContext(CreateOptions());

        var owner = await CreateTestUserAsync(context);
        var outsider = await CreateTestUserAsync(context);
        var noteToDelete = await CreateTestNoteAsync(context, owner.Id);

        var currentUserId = userIsOwner ? owner.Id : outsider.Id;

        var service = new NoteService(context, _mockMapper.Object, _mockLogger.Object);

        var deletionSucceeded = await service.DeleteNotePermanentlyAsync(noteToDelete.Id, currentUserId);
        var noteInDb = await context.Notes.FindAsync(noteToDelete.Id);

        if (userIsOwner)
        {
            Assert.True(deletionSucceeded);
            Assert.Null(noteInDb);
        }
        else
        {
            Assert.False(deletionSucceeded);
            Assert.NotNull(noteInDb);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AddCollaboratorAsync_ReturnsExpectedResult_BasedOnOwnership(bool isOwner)
    {
        await using var context = new ApplicationDbContext(CreateOptions());

        var owner = await CreateTestUserAsync(context, "owner@example.com");
        var notOwner = await CreateTestUserAsync(context, "notowner@example.com");
        var newCollaborator = await CreateTestUserAsync(context, "newuser@example.com");

        var note = await CreateTestNoteAsync(context, owner.Id);
        var actingUserId = isOwner ? owner.Id : notOwner.Id;

        var request = new AddCollaboratorRequestDto
        {
            UserEmail = newCollaborator.Email,
            Role = Role.Editor
        };

        _mockMapper.Setup(m => m.Map<NoteCollaboratorDto>(It.IsAny<NoteCollaborator>()))
                   .Returns(new NoteCollaboratorDto { UserId = newCollaborator.Id, Role = Role.Editor });

        var service = new NoteService(context, _mockMapper.Object, _mockLogger.Object);

        var result = await service.AddCollaboratorAsync(note.Id, request, actingUserId);

        if (isOwner)
        {
            Assert.NotNull(result);
            Assert.Equal(newCollaborator.Id, result.UserId);
            Assert.Equal(Role.Editor, result.Role);

            var added = await context.NoteCollaborators
                .FirstOrDefaultAsync(c => c.NoteId == note.Id && c.UserId == newCollaborator.Id);

            Assert.NotNull(added);
            Assert.Equal(Role.Editor, added.Role);
        }
        else
        {
            Assert.Null(result);

            var added = await context.NoteCollaborators
                .FirstOrDefaultAsync(c => c.NoteId == note.Id && c.UserId == newCollaborator.Id);

            Assert.Null(added);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateCollaboratorRoleAsync_ReturnsExpectedResult_BasedOnOwnership(bool isOwner)
    {
        await using var context = new ApplicationDbContext(CreateOptions());

        var owner = await CreateTestUserAsync(context);
        var notOwner = await CreateTestUserAsync(context);
        var collaborator = await CreateTestUserAsync(context, "collab@example.com");

        var note = await CreateTestNoteAsync(context, owner.Id);
        await AddTestCollaboratorAsync(context, note.Id, collaborator.Id, Role.Viewer);

        var request = new UpdateCollaboratorRoleRequestDto
        {
            NewRole = Role.Editor
        };

        var actingUserId = isOwner ? owner.Id : notOwner.Id;

        if (isOwner)
        {
            _mockMapper.Setup(m => m.Map<NoteCollaboratorDto>(It.IsAny<NoteCollaborator>()))
                       .Returns(new NoteCollaboratorDto { UserId = collaborator.Id, Role = Role.Editor });
        }

        var service = new NoteService(context, _mockMapper.Object, _mockLogger.Object);

        var result = await service.UpdateCollaboratorRoleAsync(note.Id, collaborator.Id, request, actingUserId);

        if (isOwner)
        {
            Assert.NotNull(result);
            Assert.Equal(Role.Editor, result.Role);

            var updated = await context.NoteCollaborators
                .FirstOrDefaultAsync(c => c.NoteId == note.Id && c.UserId == collaborator.Id);

            Assert.NotNull(updated);
            Assert.Equal(Role.Editor, updated.Role);
        }
        else
        {
            Assert.Null(result);

            var unchanged = await context.NoteCollaborators
                .FirstOrDefaultAsync(c => c.NoteId == note.Id && c.UserId == collaborator.Id);

            Assert.NotNull(unchanged);
            Assert.Equal(Role.Viewer, unchanged.Role);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RemoveCollaboratorAsync_BehavesCorrectly_BasedOnOwnership(bool isOwner)
    {
        await using var context = new ApplicationDbContext(CreateOptions());

        var owner = await CreateTestUserAsync(context);
        var notOwner = await CreateTestUserAsync(context);
        var collaborator = await CreateTestUserAsync(context);
        var note = await CreateTestNoteAsync(context, owner.Id);

        await AddTestCollaboratorAsync(context, note.Id, collaborator.Id);

        var service = new NoteService(context, _mockMapper.Object, _mockLogger.Object);

        var actingUserId = isOwner ? owner.Id : notOwner.Id;

        var result = await service.RemoveCollaboratorAsync(note.Id, collaborator.Id, actingUserId);

        if (isOwner)
        {
            Assert.True(result);

            var remainingCollaborators = await context.NoteCollaborators
                .Where(nc => nc.NoteId == note.Id && nc.UserId == collaborator.Id)
                .ToListAsync();

            Assert.Empty(remainingCollaborators);
        }
        else
        {
            Assert.False(result);
        }
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetNoteCollaboratorsAsync_ReturnsCorrectResult_BasedOnUserAccess(bool hasAccess)
    {
        await using var context = new ApplicationDbContext(CreateOptions());
        var owner = await CreateTestUserAsync(context);
        var outsider = await CreateTestUserAsync(context);
        var note = await CreateTestNoteAsync(context, owner.Id);

        if (hasAccess)
        {
            await AddTestCollaboratorAsync(context, note.Id, owner.Id);

            _mockMapper.Setup(m => m.Map<List<NoteCollaboratorDto>>(It.IsAny<List<NoteCollaborator>>()))
                       .Returns(
                       [
                           new() { UserId = owner.Id, Role = Role.Admin }
                       ]);
        }

        var service = new NoteService(context, _mockMapper.Object, _mockLogger.Object);

        var userId = hasAccess ? owner.Id : outsider.Id;
        var result = await service.GetNoteCollaboratorsAsync(note.Id, userId);

        if (hasAccess)
        {
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(owner.Id, result[0].UserId);
            Assert.Equal(Role.Admin, result[0].Role);
        }
        else
        {
            Assert.Null(result);
        }
    }
}
