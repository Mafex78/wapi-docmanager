namespace Shared.Domain;

public interface IAudit
{
    //string CreatedBy { get; }
    DateTime CreatedAtUtc { get; }
    //string UpdatedBy { get; }
    DateTime? UpdatedAtUtc { get; }
}