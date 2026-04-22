using Shared.Domain.Types;
using WAPIIdentity.Domain.Entities;

namespace WAPIIdentity.Application.Dto;

public record RegisterUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password  { get; set; } = string.Empty;
    public IList<RoleType> Roles { get; set; } =  new List<RoleType>();

    public static explicit operator User(RegisterUserRequest model)
    {
        return new User()
        {
            Email = model.Email,
            Password = model.Password, // Meglio fare l'hash!!
            Roles = model.Roles
        };
    }
}