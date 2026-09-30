using WAPIDocManager.UI.Features.Auth.Models;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Auth.Services;

public interface IAuthService
{
    /// Esegue l'accesso e salva la sessione ottenuta.
    /// Il salvataggio aggiorna lo stato di autenticazione dell'applicazione: l'interfaccia reagisce da sola,
    /// senza che il chiamante debba fare altro.
    /// Credenziali errate o utente non attivo arrivano come eccezione con stato 401.
    Task<UserSession> LoginAsync(LoginModel model, CancellationToken cancellationToken = default);

    /// Termina la sessione locale. Il token è stateless: non esiste un endpoint di uscita lato server.
    Task LogoutAsync();
}
