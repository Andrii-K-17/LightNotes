namespace LightNotes.Domain.Entities;

///<summary>
/// Базовий абстрактний клас для сутностей
///</summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Унікальний ідентифікатор
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Дата створення у UTC
}
