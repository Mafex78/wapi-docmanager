using System.Security.Claims;
using WAPIDocManager.UI.Features.Documents;

namespace WAPIDocManager.UI.Shared.Authentication;

/// <summary>
/// Utility sul ClaimsPrincipal costruito da <see cref="JwtAuthenticationStateProvider"/>.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Ruoli dell'utente come enum, da passare a <c>Features/Documents/DocumentPermissions</c>.
    /// </summary>
    /// <remarks>
    /// Uso tipico nelle pagine: <c>_roles = (await AuthenticationStateTask).User.GetRoles();</c> in OnInitializedAsync,
    /// con <c>[CascadingParameter] Task&lt;AuthenticationState&gt; AuthenticationStateTask</c>. Claim non riconosciuti vengono ignorati.
    /// </remarks>
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
