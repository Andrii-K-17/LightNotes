using LightNotes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LightNotes.Infrastructure.Data;

/// <summary>
/// Контекст бази даних для EF Core, описує таблиці та зв'язки.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // Таблиці в базі даних
    public DbSet<User> Users { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<NoteCollaborator> NoteCollaborators { get; set; }
    public DbSet<NoteTag> NoteTags { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    // Налаштування моделі БД
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User: унікальний індекс на Email
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        // Note: зв'язок із власником (Owner)
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasOne(n => n.Owner)
                  .WithMany(u => u.OwnedNotes)
                  .HasForeignKey(n => n.OwnerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // NoteCollaborator: багато-до-багатьох між Note і User
        modelBuilder.Entity<NoteCollaborator>(entity =>
        {
            entity.HasKey(nc => new { nc.NoteId, nc.UserId });

            entity.HasOne(nc => nc.Note)
                  .WithMany(n => n.Collaborators)
                  .HasForeignKey(nc => nc.NoteId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(nc => nc.User)
                  .WithMany(u => u.Collaborations)
                  .HasForeignKey(nc => nc.UserId)
                  .OnDelete(DeleteBehavior.Restrict); // зберегти нотатки при видаленні користувача
        });

        // NoteTag: багато-до-багатьох між Note і Tag
        modelBuilder.Entity<NoteTag>(entity =>
        {
            entity.HasKey(nt => new { nt.NoteId, nt.Tag });

            entity.HasOne(nt => nt.Note)
                  .WithMany(n => n.Tags)
                  .HasForeignKey(nt => nt.NoteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ChatMessage: зв’язки з Note і User (Sender)
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasOne(cm => cm.Note)
                  .WithMany(n => n.ChatMessages)
                  .HasForeignKey(cm => cm.NoteId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cm => cm.Sender)
                  .WithMany(u => u.SentMessages)
                  .HasForeignKey(cm => cm.SenderId)
                  .OnDelete(DeleteBehavior.Restrict); // зберегти повідомлення після видалення користувача
        });
    }
}
