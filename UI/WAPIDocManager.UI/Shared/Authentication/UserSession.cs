namespace WAPIDocManager.UI.Shared.Authentication;

/// Sessione dell'utente autenticato: identità, token e ruoli validi per la durata dell'accesso.
/// È serializzata come JSON nell'archivio di sessione del browser (SessionStorageUserSessionStore): rinominare una
/// proprietà rende illeggibili le sessioni già salvate, e l'utente deve rifare il login.
/// Da qui JwtAuthenticationStateProvider costruisce l'identità su cui si basano le autorizzazioni delle pagine
/// e le voci di menu.
public record UserSession
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    /// JWT inviato nell'header Authorization a ogni chiamata verso le API protette.
    public string Token { get; init; } = string.Empty;

    /// Scadenza del token in UTC: di default 60 minuti dal login.
    public DateTime ExpirationUtc { get; init; }

    /// Ruoli letti dal payload del JWT: la risposta del login non li restituisce esplicitamente.
    public IReadOnlyList<RoleType> Roles { get; init; } = new List<RoleType>();

    /// Le API non applicano tolleranza sulla scadenza: nell'istante in cui scade, il token è già rifiutato.
    public bool IsExpired(DateTime utcNow)
    {
        return utcNow >= ExpirationUtc;
    }

    public bool IsInRole(RoleType role)
    {
        return Roles.Contains(role);
    }
}
