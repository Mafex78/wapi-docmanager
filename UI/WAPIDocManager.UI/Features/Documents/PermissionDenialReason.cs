namespace WAPIDocManager.UI.Features.Documents;

/// Motivo per cui un'azione sul documento non è consentita.
/// L'interfaccia lo traduce nel testo che spiega all'utente perché un pulsante è disattivato, invece di lasciarlo
/// senza risposta davanti a un comando inerte.
public enum PermissionDenialReason
{
    /// Azione consentita.
    None = 0,

    /// All'utente manca un ruolo con diritti di scrittura.
    MissingRole = 1,

    /// Lo stato in cui si trova il documento non consente l'azione.
    InvalidStatus = 2
}
