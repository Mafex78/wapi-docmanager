using WAPIDocManager.UI.Features.Auth.Services;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Shared.Authentication;

/// <summary>
/// Persistenza della sessione utente lato browser
/// </summary>
/// <remarks>
/// <para>
/// Implementazione: <c>Shared/Authentication/SessionStorageUserSessionStore</c> (sessionStorage, scelta concordata:
/// la sessione vale solo per la scheda aperta).
/// </para>
/// <para>
/// È il punto di coordinamento dell'autenticazione, condiviso da:
/// <c>AuthApiService</c> (Set/Clear), <c>BearerTokenHandler</c> (Get, Clear su 401/scadenza)
/// e <c>JwtAuthenticationStateProvider</c> (Get + ascolto di <see cref="SessionChanged"/>).
/// Per questo deve essere registrato Singleton.
/// </para>
/// </remarks>
public interface IUserSessionStore
{
    /// <summary>
    /// Sollevato a ogni login/logout (null = sessione terminata)
    /// </summary>
    event Action<UserSession?>? SessionChanged;

    /// <summary>Restituisce la sessione salvata (null se assente o illeggibile).</summary>
    ValueTask<UserSession?> GetAsync();

    ValueTask SetAsync(UserSession session);

    ValueTask ClearAsync();
}
