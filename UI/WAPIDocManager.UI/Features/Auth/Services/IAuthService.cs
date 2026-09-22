using WAPIDocManager.UI.Features.Auth.Models;
using WAPIDocManager.UI.Shared.Api;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Auth.Services;

/// <summary>
/// Autenticazione dell'utente su WAPIIdentity.
/// </summary>
/// <remarks>
/// Implementazione: <c>Features/Auth/Services/AuthApiService</c> (typed HttpClient verso IdentityBaseUrl, SENZA BearerTokenHandler).
/// </remarks>
public interface IAuthService
{
    /// <summary>
    /// Esegue <c>POST api/v1/auth/login</c> e salva la sessione in <see cref="IUserSessionStore"/>.
    /// </summary>
    /// <remarks>
    /// Il salvataggio solleva <see cref="IUserSessionStore.SessionChanged"/>, che aggiorna lo stato di autenticazione della UI.
    /// Credenziali errate o utente non attivo: <c>ApiException</c> con status 401.
    /// </remarks>
    Task<UserSession> LoginAsync(LoginModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Termina la sessione locale. Il JWT è stateless: non esiste un endpoint di logout lato server.
    /// </summary>
    Task LogoutAsync();
}
