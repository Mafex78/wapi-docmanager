namespace Shared.Domain;

public interface IEntity<T>
{
    T Id { get; }
    long Version { get; set; }
}