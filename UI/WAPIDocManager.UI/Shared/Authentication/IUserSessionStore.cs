namespace WAPIDocManager.UI.Shared.Authentication;

/// Persistenza della sessione utente nel browser: chi ha fatto l'accesso, con quale token e per quanto ancora.
/// Va registrata Singleton. Ci accedono sia l'interfaccia sia i gestori delle chiamate HTTP, che vengono risolti in
/// uno scope di dipendenze separato: registrata Scoped, i due lati leggerebbero due sessioni diverse.
/// L'archivio scelto vale per la singola scheda del browser, quindi la sessione non è condivisa fra schede.
public interface IUserSessionStore
{
    /// Sollevato a ogni accesso e uscita; null significa sessione terminata.
    /// È il segnale che tiene allineato lo stato di autenticazione dell'applicazione.
    event Action<UserSession?>? SessionChanged;

    /// Restituisce la sessione salvata, o null se assente o illeggibile.
    ValueTask<UserSession?> GetAsync();

    ValueTask SetAsync(UserSession session);

    ValueTask ClearAsync();
}
