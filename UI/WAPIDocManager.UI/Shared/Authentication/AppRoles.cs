namespace WAPIDocManager.UI.Shared.Authentication;

/// Ruoli usati negli attributi di autorizzazione delle pagine e nei blocchi di interfaccia condizionati dal ruolo.
/// Più ruoli separati da virgola significano "almeno uno". Devono corrispondere alle autorizzazioni dichiarate dalle
/// API: se divergono, l'utente vede pagine e pulsanti che il server poi rifiuta.
/// Gli stessi ruoli sono controllati anche nel codice delle pagine, da DocumentPermissions: sono due meccanismi
/// paralleli e vanno tenuti allineati.
public static class AppRoles
{
    /// Registrazione utenti.
    public const string Admin = nameof(RoleType.Admin);

    /// Lettura documenti. Editor è stato aggiunto alle letture insieme alla creazione dell'interfaccia.
    public const string Readers = $"{nameof(RoleType.Viewer)},{nameof(RoleType.Editor)},{nameof(RoleType.Admin)}";

    /// Creazione, modifica, cambio stato, generazione, collegamento ed eliminazione documenti.
    public const string Writers = $"{nameof(RoleType.Editor)},{nameof(RoleType.Admin)}";
}
