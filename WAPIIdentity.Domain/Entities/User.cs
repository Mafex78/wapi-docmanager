using Shared.Domain;
using Shared.Domain.Types;

namespace WAPIIdentity.Domain.Entities;

public class User : IEntity<string>
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public long Version { get; set; }
    public bool IsActive { get; set; }
    public string Email { get; set; } =  string.Empty;
    public string Password { get; set; } = string.Empty;
    public IList<RoleType> Roles { get; set; }  = new List<RoleType>();
}