namespace Shared.Domain;

public interface IEntity<T>
{
    T Id { get; set; }
    DateTime CreatedAtUtc { get; set; }
    DateTime? UpdatedAtUtc { get; set; }
    long Version { get; set; }
}