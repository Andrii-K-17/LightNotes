namespace LightNotes.Domain.Entities;

///<summary>
/// Базовий абстрактний клас для сутностей
///</summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
