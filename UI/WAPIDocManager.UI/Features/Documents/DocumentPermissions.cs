using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Documents;

/// <summary>
/// Azioni consentite in base a ruoli dell'utente (autorizzazioni delle API) e stato del documento (regole di dominio)
/// </summary>
/// <remarks>
/// <para>
/// Serve solo a mostrare/nascondere pulsanti e link: l'autorizzazione effettiva è sempre verificata dalle API.
/// </para>
/// <para>
/// Ruoli copiati dagli attributi <c>[Authorize(Roles = ...)]</c> di <c>WAPIDocument/Controllers/DocumentsController.cs</c>
/// e <c>WAPIIdentity/Controllers/UsersController.cs</c>. Se cambiano lì vanno aggiornati qui, in
/// <c>WAPIDocManager.UI/Shared/Authentication/AppRoles.cs</c> (attributi delle pagine) e nei test <c>DocumentPermissionsTests</c>.
/// </para>
/// <para>
/// I ruoli arrivano dal ClaimsPrincipal corrente tramite <c>ClaimsPrincipalExtensions.GetRoles()</c> (progetto UI).
/// </para>
/// </remarks>
public static class DocumentPermissions
{
    /// <summary>GET api/v1/documents e GET api/v1/documents/{id}.</summary>
    public static bool CanRead(IReadOnlyCollection<RoleType> roles)
    {
        return roles.Any(role => role is RoleType.Viewer or RoleType.Editor or RoleType.Admin);
    }

    /// <summary>Creazione, modifica, cambio stato, generazione, collegamento, eliminazione.</summary>
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

    /// <summary>Vero se l'utente può scrivere ed esiste almeno una transizione dallo stato corrente.</summary>
    public static bool CanChangeStatus(IReadOnlyCollection<RoleType> roles, DocumentStatus status)
    {
        return CanWrite(roles) && DocumentRules.GetNextStatuses(status).Count > 0;
    }

    /// <summary>
    /// Motivo per cui la modifica non è consentita (<see cref="PermissionDenialReason.None"/> se consentita).
    /// </summary>
    /// <remarks>
    /// Il ruolo ha la precedenza sullo stato: per un utente senza ruolo di scrittura il motivo è sempre MissingRole.
    /// Coerente con <see cref="CanEdit"/>: None se e solo se CanEdit è vero.
    /// </remarks>
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

    /// <summary>
    /// Motivo per cui l'eliminazione non è consentita (<see cref="PermissionDenialReason.None"/> se consentita).
    /// </summary>
    /// <remarks>
    /// Il ruolo ha la precedenza sullo stato: per un utente senza ruolo di scrittura il motivo è sempre MissingRole.
    /// Coerente con <see cref="CanDelete"/>: None se e solo se CanDelete è vero.
    /// </remarks>
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

    /// <summary>POST api/v1/users/register.</summary>
    public static bool CanRegisterUsers(IReadOnlyCollection<RoleType> roles)
    {
        return roles.Contains(RoleType.Admin);
    }
}
