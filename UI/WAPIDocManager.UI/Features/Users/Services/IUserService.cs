using WAPIDocManager.UI.Features.Users.Models;

namespace WAPIDocManager.UI.Features.Users.Services;

public interface IUserService
{
    /// Registra un nuovo utente e ne restituisce l'identificativo. È riservata agli amministratori.
    /// L'unicità dell'email la verifica solo il server: se l'indirizzo è già in uso la richiesta viene rifiutata e
    /// il messaggio arriva con l'errore. Non è una verifica replicabile qui, perché richiederebbe di conoscere tutti
    /// gli utenti.
    Task<string?> RegisterAsync(RegisterUserModel model, CancellationToken cancellationToken = default);
}
