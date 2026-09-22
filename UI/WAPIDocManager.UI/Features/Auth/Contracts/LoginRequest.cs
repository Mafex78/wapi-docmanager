using WAPIDocManager.UI.Features.Auth.Models;
using WAPIDocManager.UI.Features.Auth.Services;

namespace WAPIDocManager.UI.Features.Auth.Contracts;

/// <summary>
/// Body di <c>POST api/v1/auth/login</c> (<c>WAPIIdentity.Application/Dto/LoginRequest.cs</c>).
/// </summary>
/// <remarks>
/// Copia lato client del DTO del backend (il client non può referenziare WAPIIdentity.Application):
/// se cambia sul server va aggiornato qui e nei test di AuthApiService.
/// </remarks>
public record LoginRequest
{
    public string? Email { get; init; }
    public string? Password { get; init; }

    /// <summary>
    /// Form di login → body (email con trim, password invariata).
    /// </summary>
    public static explicit operator LoginRequest(LoginModel model)
    {
        return new LoginRequest
        {
            Email = model.Email?.Trim(),
            Password = model.Password
        };
    }
}
