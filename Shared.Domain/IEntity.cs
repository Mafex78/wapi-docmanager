namespace Shared.Domain;

public interface IEntity<T>
{
    T Id { get; set; }
    long Version { get; set; }
}