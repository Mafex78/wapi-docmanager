using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Users.Models;

/// Modello del form di registrazione di un utente. Non porta attributi di validazione: le regole stanno nel
/// validatore.
public class RegisterUserModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }

    /// Ruoli da assegnare. Anche nessuno è accettato dal server, e gli eventuali duplicati vengono tolti prima
    /// dell'invio.
    public List<RoleType> Roles { get; set; } = new();
}
