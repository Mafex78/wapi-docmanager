using WAPIDocManager.UI.Features.Users.Models;
using WAPIDocManager.UI.Features.Users.Services;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Users.Contracts;

/// <summary>
/// Body di <c>POST api/v1/users/register</c> (<c>WAPIIdentity.Application/Dto/RegisterUserRequest.cs</c>), ruoli come numeri.
/// </summary>
/// <remarks>Se il DTO cambia sul server va aggiornato qui e nei test di UserApiService.</remarks>
public record RegisterUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public IList<RoleType> Roles { get; init; } = new List<RoleType>();

    /// <summary>
    /// Form di registrazione → body (email con trim, valori null come stringa vuota, ruoli senza duplicati).
    /// </summary>
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
