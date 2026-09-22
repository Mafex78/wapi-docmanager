using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Shared.Authentication;

/// <summary>
/// Stato di autenticazione ricavato dalla sessione JWT salvata nel browser
/// </summary>
/// <remarks>
/// <para>
/// Flusso: login, logout o 401 → <see cref="IUserSessionStore.SessionChanged"/> → NotifyAuthenticationStateChanged →
/// AuthorizeRouteView e AuthorizeView si aggiornano (in App.razor l'utente anonimo viene mandato a RedirectToLogin).
/// </para>
/// <para>
/// Claims prodotti: NameIdentifier = UserId; Name ed Email = email (mostrata nella top bar); un claim Role per ogni ruolo
/// con il nome dell'enum, letto da [Authorize(Roles)], AuthorizeView Roles e <see cref="ClaimsPrincipalExtensions.GetRoles"/>.
/// </para>
/// <para>
/// La scadenza del token è verificata all'avvio dell'app (qui) e a ogni chiamata API (BearerTokenHandler):
/// non esiste un timer che disconnette l'utente inattivo.
/// </para>
/// </remarks>
public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    // un valore qualsiasi purché non vuoto: senza authenticationType ClaimsIdentity.IsAuthenticated è false
    private const string AuthenticationType = "jwt";

    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly IUserSessionStore _sessionStore;
    private readonly TimeProvider _timeProvider;

    public JwtAuthenticationStateProvider(
        IUserSessionStore sessionStore,
        TimeProvider timeProvider)
    {
        _sessionStore = sessionStore;
        _timeProvider = timeProvider;
        _sessionStore.SessionChanged += OnSessionChanged;
    }

    /// <summary>
    /// Chiamato da CascadingAuthenticationState all'avvio; gli aggiornamenti successivi arrivano tramite OnSessionChanged.
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        UserSession? session = await _sessionStore.GetAsync();

        if (session is null)
        {
            return Anonymous;
        }

        // sessione rimasta nel sessionStorage oltre la scadenza (es. reload dopo più di un'ora)
        if (session.IsExpired(_timeProvider.GetUtcNow().UtcDateTime))
        {
            await _sessionStore.ClearAsync();
            return Anonymous;
        }

        return BuildState(session);
    }

    public void Dispose()
    {
        _sessionStore.SessionChanged -= OnSessionChanged;
    }

    // la nuova sessione è già disponibile nell'evento: nessuna rilettura dallo storage
    private void OnSessionChanged(UserSession? session)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(BuildState(session)));
    }

    private static AuthenticationState BuildState(UserSession? session)
    {
        if (session is null)
        {
            return Anonymous;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.UserId),
            new(ClaimTypes.Name, session.Email),
            new(ClaimTypes.Email, session.Email)
        };

        claims.AddRange(session.Roles.Select(role => new Claim(ClaimTypes.Role, role.ToString())));

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, AuthenticationType)));
    }
}
