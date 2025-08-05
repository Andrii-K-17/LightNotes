using System;
using System.Threading.Tasks;
using LightNotes.Domain.Entities;
using LightNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;
using Xunit;

namespace LightNotes.Tests.Integration.Data;

public class ApplicationDbContextTests : IAsyncLifetime
{
    private readonly MySqlContainer _mySqlContainer;
    private DbContextOptions<ApplicationDbContext>? _options;
    
    public ApplicationDbContextTests()
    {
        _mySqlContainer = new MySqlBuilder()
            .WithDatabase("lightnotes_test")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mySqlContainer.StartAsync();

        var connectionString = _mySqlContainer.GetConnectionString();
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        using var context = new ApplicationDbContext(_options);
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _mySqlContainer.StopAsync();
    }

    private ApplicationDbContext CreateContext()
    {
        if (_options == null)
        {
            throw new Exception("DbContext options are not initialized");
        }
        return new ApplicationDbContext(_options);
    }

    [Fact]
    public async Task AddNote_WithOwner_NoteAndOwnerAreSavedCorrectly()
    {
        await using var context = CreateContext();
        
        var testUser = new User { Email = "test@example.com" };
        var testNote = new Note { Title = "Test Note", Owner = testUser };

        context.Users.Add(testUser);
        context.Notes.Add(testNote);
        await context.SaveChangesAsync();

        var savedNote = await context.Notes.Include(n => n.Owner).FirstOrDefaultAsync();

        Assert.NotNull(savedNote);
        Assert.Equal("Test Note", savedNote.Title);
        Assert.NotNull(savedNote.Owner);
        Assert.Equal("test@example.com", savedNote.Owner.Email);
    }

    [Fact]
    public async Task DeletingUserCascade_DeletesOwnedNotes()
    {
        var userId = Guid.NewGuid();

        await using var context1 = CreateContext();
        var user = new User { Id = userId, Email = "owner@example.com" };
        var note = new Note { Title = "Note A", OwnerId = userId };
        
        context1.Users.Add(user);
        context1.Notes.Add(note);
        await context1.SaveChangesAsync();

        await using var context2 = CreateContext();
        var userToDelete = await context2.Users.FindAsync(userId);
        context2.Users.Remove(userToDelete!);
        await context2.SaveChangesAsync();

        await using var context3 = CreateContext();
        var notesCount = await context3.Notes.CountAsync();
        Assert.Equal(0, notesCount);
    }

    [Fact]
    public async Task ThrowException_IfTwoUsersHaveSameEmail()
    {
        await using var context = CreateContext();

        var email = "duplicate@example.com";
        var user1 = new User { Email = email };
        var user2 = new User { Email = email };

        context.Users.Add(user1);
        await context.SaveChangesAsync();

        context.Users.Add(user2);
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
