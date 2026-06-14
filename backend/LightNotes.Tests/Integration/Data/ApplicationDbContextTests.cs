using LightNotes.Domain.Entities;
using LightNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;

namespace LightNotes.Tests.Integration.Data;

/// <summary>
/// Integration tests for ApplicationDbContext utilizing an isolated, containerized MySQL database.
/// </summary>
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

    public async ValueTask InitializeAsync()
    {
        await _mySqlContainer.StartAsync();

        var connectionString = _mySqlContainer.GetConnectionString();
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        await using var context = new ApplicationDbContext(_options);
        await context.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _mySqlContainer.StopAsync();

        GC.SuppressFinalize(this);
    }

    private ApplicationDbContext CreateContext()
    {
        if (_options == null)
        {
            throw new InvalidOperationException("DbContext options are not initialized");
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
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var savedNote = await context.Notes.Include(n => n.Owner).FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

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
        await context1.SaveChangesAsync(TestContext.Current.CancellationToken);

        await using var context2 = CreateContext();
        var userToDelete = await context2.Users.FindAsync([userId], TestContext.Current.CancellationToken);
        context2.Users.Remove(userToDelete!);
        await context2.SaveChangesAsync(TestContext.Current.CancellationToken);

        await using var context3 = CreateContext();
        var notesCount = await context3.Notes.CountAsync(cancellationToken: TestContext.Current.CancellationToken);
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
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.Users.Add(user2);
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }
}
