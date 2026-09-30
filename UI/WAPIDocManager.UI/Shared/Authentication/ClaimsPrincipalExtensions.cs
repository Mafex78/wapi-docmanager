using System.Security.Claims;

namespace WAPIDocManager.UI.Shared.Authentication;

/// Utility sull'identità dell'utente autenticato.
public static class ClaimsPrincipalExtensions
{
    /// Ruoli dell'utente come enum, nella forma attesa dai controlli di permesso delle pagine.
    /// I claim che non corrispondono a un ruolo noto vengono ignorati.
    /// Uso tipico in una pagina, dentro OnInitializedAsync:
    ///     _roles = (await AuthenticationStateTask).User.GetRoles();
    /// dove AuthenticationStateTask è lo stato di autenticazione ricevuto come parametro a cascata.
    public static IReadOnlyCollection<RoleType> GetRoles(this ClaimsPrincipal user)
    {
        var roles = new List<RoleType>();

        foreach (Claim claim in user.FindAll(ClaimTypes.Role))
        {
            if (Enum.TryParse(claim.Value, out RoleType role) && !roles.Contains(role))
            {
                roles.Add(role);
            }
        }

        return roles;
    }
}
