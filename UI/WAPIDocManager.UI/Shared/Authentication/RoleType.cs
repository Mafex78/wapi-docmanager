namespace WAPIDocManager.UI.Shared.Authentication;

/// Ruoli applicativi assegnati all'utente dal servizio di autenticazione.
/// I valori numerici fanno parte del contratto: viaggiano come numeri nel body di POST api/v1/users/register.
/// Anche i NOMI fanno parte del contratto: compaiono come stringhe nel claim di ruolo del JWT e negli attributi di
/// autorizzazione, sia qui sia sulle API. Rinominare un valore rompe l'autorizzazione su entrambi i lati.
public enum RoleType
{
    Admin = 0,
    Editor = 1,
    Viewer = 2
}
