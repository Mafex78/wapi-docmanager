using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Documents;

/// Azioni consentite, incrociando i ruoli dell'utente con lo stato del documento.
/// Serve solo a mostrare o nascondere pulsanti e collegamenti: l'autorizzazione vera la verificano sempre le API.
/// Qui si evita all'utente di provare un'azione che verrebbe rifiutata.
/// I ruoli sono la copia di quelli dichiarati dalle API. Se cambiano là vanno aggiornati anche qui, negli attributi
/// delle pagine e nei test dei permessi.
public static class DocumentPermissions
{
    /// Lettura dell'elenco e del singolo documento.
    public static bool CanRead(IReadOnlyCollection<RoleType> roles)
    {
        return roles.Any(role => role is RoleType.Viewer or RoleType.Editor or RoleType.Admin);
    }

    /// Creazione, modifica, cambio di stato, generazione, collegamento ed eliminazione.
    public static bool CanWrite(IReadOnlyCollection<RoleType> roles)
    {
        return roles.Any(role => role is RoleType.Editor or RoleType.Admin);
    }

    public static bool CanEdit(IReadOnlyCollection<RoleType> roles, DocumentStatus status)
    {
        return CanWrite(roles) && DocumentRules.CanEdit(status);
    }

    public static bool CanDelete(IReadOnlyCollection<RoleType> roles, DocumentStatus status)
    {
        return CanWrite(roles) && DocumentRules.CanDelete(status);
    }

    /// Vero se l'utente può scrivere ed esiste almeno uno stato successivo raggiungibile da quello corrente.
    public static bool CanChangeStatus(IReadOnlyCollection<RoleType> roles, DocumentStatus status)
    {
        return CanWrite(roles) && DocumentRules.GetNextStatuses(status).Count > 0;
    }

    /// Motivo per cui la modifica non è consentita, o nessun motivo se lo è.
    /// Il ruolo ha la precedenza sullo stato: a chi non può scrivere si risponde sempre che gli manca il ruolo,
    /// perché è l'ostacolo che conta. Resta coerente con il controllo secco: nessun motivo se e solo se la modifica
    /// è consentita.
    public static PermissionDenialReason GetEditDenialReason(IReadOnlyCollection<RoleType> roles, DocumentStatus status)
    {
        if (!CanWrite(roles))
        {
            return PermissionDenialReason.MissingRole;
        }

        return DocumentRules.CanEdit(status)
            ? PermissionDenialReason.None
            : PermissionDenialReason.InvalidStatus;
    }

    /// Motivo per cui l'eliminazione non è consentita, o nessun motivo se lo è.
    /// Vale la stessa precedenza del ruolo sullo stato descritta sopra.
    public static PermissionDenialReason GetDeleteDenialReason(IReadOnlyCollection<RoleType> roles, DocumentStatus status)
    {
        if (!CanWrite(roles))
        {
            return PermissionDenialReason.MissingRole;
        }

        return DocumentRules.CanDelete(status)
            ? PermissionDenialReason.None
            : PermissionDenialReason.InvalidStatus;
    }

    /// Registrazione di nuovi utenti.
    public static bool CanRegisterUsers(IReadOnlyCollection<RoleType> roles)
    {
        return roles.Contains(RoleType.Admin);
    }
}
