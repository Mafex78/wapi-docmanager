using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents;

/// <summary>
/// Motivo per cui un'azione sul documento non è consentita.
/// </summary>
/// <remarks>
/// Restituito da <see cref="DocumentPermissions.GetEditDenialReason"/> e <see cref="DocumentPermissions.GetDeleteDenialReason"/>:
/// la UI lo traduce nel testo del tooltip dei pulsanti disabilitati (chiavi <c>Permission_*</c> nei .resx).
/// </remarks>
public enum PermissionDenialReason
{
    /// <summary>Azione consentita.</summary>
    None = 0,

    /// <summary>L'utente non ha un ruolo di scrittura (Editor o Admin).</summary>
    MissingRole = 1,

    /// <summary>Lo stato del documento non consente l'azione (vedi <c>DocumentRules</c>).</summary>
    InvalidStatus = 2
}
