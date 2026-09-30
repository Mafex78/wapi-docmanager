using WAPIDocManager.UI.Features.Users.Models;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Users.Services.Dto;

/// Body di POST api/v1/users/register. I ruoli viaggiano come numeri.
public record RegisterUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public IList<RoleType> Roles { get; init; } = new List<RoleType>();

    /// Dal form al corpo della richiesta: email ripulita dagli spazi, campi non compilati inviati come stringa
    /// vuota, ruoli senza duplicati.
    public static explicit operator RegisterUserRequest(RegisterUserModel model)
    {
        return new RegisterUserRequest
        {
            Email = model.Email?.Trim() ?? string.Empty,
            Password = model.Password ?? string.Empty,
            Roles = model.Roles.Distinct().ToList()
        };
    }
}
