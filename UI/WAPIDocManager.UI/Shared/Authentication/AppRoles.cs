using WAPIDocManager.UI.Features.Documents;

namespace WAPIDocManager.UI.Shared.Authentication;

/// <summary>
/// Ruoli per gli attributi [Authorize] e AuthorizeView, allineati alle autorizzazioni delle API
/// </summary>
/// <remarks>
/// Più ruoli separati da virgola significano "almeno uno". Devono corrispondere agli <c>[Authorize(Roles = ...)]</c>
/// di <c>WAPIDocument/Controllers/DocumentsController.cs</c> e <c>WAPIIdentity/Controllers/UsersController.cs</c>
/// e a <c>Features/Documents/DocumentPermissions</c>, che fa lo stesso controllo nel codice dei componenti.
/// </remarks>
public static class AppRoles
{
    /// <summary>Registrazione utenti.</summary>
    public const string Admin = nameof(RoleType.Admin);

    /// <summary>Lettura documenti (Editor è stato aggiunto ai GET di DocumentsController insieme alla creazione della UI).</summary>
    public const string Readers = $"{nameof(RoleType.Viewer)},{nameof(RoleType.Editor)},{nameof(RoleType.Admin)}";

    /// <summary>Creazione, modifica, cambio stato, generazione, collegamento ed eliminazione documenti.</summary>
    public const string Writers = $"{nameof(RoleType.Editor)},{nameof(RoleType.Admin)}";
}
