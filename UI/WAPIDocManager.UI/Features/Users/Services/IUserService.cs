using WAPIDocManager.UI.Features.Users.Models;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Users.Services;

/// <summary>
/// Gestione utenti su WAPIIdentity (<c>WAPIIdentity/Controllers/UsersController.cs</c>).
/// </summary>
/// <remarks>
/// Implementazione: <c>Features/Users/Services/UserApiService</c> (typed HttpClient verso IdentityBaseUrl con BearerTokenHandler).
/// </remarks>
public interface IUserService
{
    /// <summary>
    /// Registra un nuovo utente e ne restituisce l'Id
    /// </summary>
    /// <remarks>
    /// <c>POST api/v1/users/register</c>, solo ruolo Admin. L'unicità dell'email è verificata solo dal server
    /// (<c>RegisterUserRequestValidator</c>): se già esistente restituisce 400 con il messaggio nel detail.
    /// </remarks>
    Task<string?> RegisterAsync(RegisterUserModel model, CancellationToken cancellationToken = default);
}
