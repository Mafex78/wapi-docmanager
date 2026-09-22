namespace WAPIDocManager.UI.Shared.Authentication;

/// <summary>
/// Ruoli applicativi assegnati all'utente da WAPIIdentity.
/// </summary>
/// <remarks>
/// <para>
/// Valori numerici identici a <c>Shared.Domain/Types/RoleType.cs</c> (usati come numeri nel body di <c>POST api/v1/users/register</c>).
/// </para>
/// <para>
/// Anche i NOMI fanno parte del contratto: compaiono come stringhe nel claim <c>role</c> del JWT,
/// negli attributi <c>[Authorize(Roles = ...)]</c> delle API e in <c>WAPIDocManager.UI/Shared/Authentication/AppRoles.cs</c>.
/// Rinominare un valore rompe l'autorizzazione lato client e lato server.
/// </para>
/// </remarks>
public enum RoleType
{
    Admin = 0,
    Editor = 1,
    Viewer = 2
}
