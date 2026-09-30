using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace WAPIDocManager.UI.Shared.Authentication;

/// Stato di autenticazione dell'applicazione, ricavato dalla sessione salvata nel browser.
/// Accesso, uscita e token rifiutato sollevano tutti il cambio di sessione, che da qui diventa una notifica di stato:
/// le pagine protette e le parti di interfaccia legate ai ruoli si aggiornano da sole, e l'utente anonimo finisce
/// alla pagina di accesso.
/// Claim prodotti: identificativo utente, nome ed email (è l'email quella mostrata nella barra in alto) e un claim
/// di ruolo per ogni ruolo, con il nome dell'enum.
/// La scadenza del token viene verificata all'avvio dell'applicazione e prima di ogni chiamata alle API: non esiste
/// un timer che disconnette l'utente inattivo.
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

    /// Chiamato una volta all'avvio; gli aggiornamenti successivi arrivano dall'evento di cambio sessione.
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        UserSession? session = await _sessionStore.GetAsync();

        if (session is null)
        {
            return Anonymous;
        }

        // sessione rimasta nell'archivio del browser oltre la scadenza, per esempio un ricaricamento dopo più di un'ora
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
