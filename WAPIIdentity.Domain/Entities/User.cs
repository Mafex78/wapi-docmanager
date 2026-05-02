using Shared.Domain;
using Shared.Domain.Types;

namespace WAPIIdentity.Domain.Entities;

public class User : IEntity<string>, IAudit
{
    public string Id { get; set; } = string.Empty;
    //public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    //public string UpdatedBy { get; private set; } = string.Empty;
    public DateTime? UpdatedAtUtc { get; private set; }
    public long Version { get; set; }
    public bool IsActive { get; set; }
    public string Email { get; set; } =  string.Empty;
    public string Password { get; set; } = string.Empty;
    public IList<RoleType> Roles { get; set; }  = new List<RoleType>();

    public void Create()
    {
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}