using System.Text.Json;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Auth.Services;

/// <summary>
/// Lettura dei ruoli dal payload del JWT emesso da WAPIIdentity (nessuna verifica della firma: la fa il server)
/// </summary>
/// <remarks>
/// <para>
/// <c>WAPIIdentity.Infrastructure/JwtTokenService.cs</c> aggiunge un claim <c>ClaimTypes.Role</c> per ogni ruolo;
/// <c>JwtSecurityTokenHandler</c> lo scrive nel payload con il nome breve <c>"role"</c> (outbound claim type map):
/// stringa se il ruolo è uno solo, array se sono più di uno. Si accetta anche il nome lungo per sicurezza.
/// </para>
/// <para>
/// Implementazione manuale (base64url + System.Text.Json) per non aggiungere System.IdentityModel.Tokens.Jwt al bundle WebAssembly.
/// I ruoli servono solo alla UI: un token manomesso verrebbe comunque rifiutato dalle API.
/// </para>
/// </remarks>
public static class JwtPayloadReader
{
    private const string RoleClaim = "role";
    private const string RoleClaimLongName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    /// <summary>
    /// Restituisce i ruoli noti presenti nel token (senza duplicati); token malformato o ruoli sconosciuti → ignorati.
    /// </summary>
    public static IReadOnlyList<RoleType> ReadRoles(string token)
    {
        // formato JWT: header.payload.signature
        string[] segments = token.Split('.');

        if (segments.Length < 2)
        {
            return Array.Empty<RoleType>();
        }

        try
        {
            using JsonDocument payload = JsonDocument.Parse(DecodeBase64Url(segments[1]));

            if (payload.RootElement.ValueKind != JsonValueKind.Object)
            {
                return Array.Empty<RoleType>();
            }

            var roles = new List<RoleType>();

            foreach (JsonProperty property in payload.RootElement.EnumerateObject())
            {
                if (property.Name != RoleClaim && property.Name != RoleClaimLongName)
                {
                    continue;
                }

                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement element in property.Value.EnumerateArray())
                    {
                        AddRole(element, roles);
                    }
                }
                else
                {
                    AddRole(property.Value, roles);
                }
            }

            return roles.Distinct().ToList();
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return Array.Empty<RoleType>();
        }
    }

    // Il claim contiene il NOME del ruolo (RoleType.ToString() lato server).
    // Enum.IsDefined scarta i valori numerici non validi che Enum.TryParse accetterebbe (es. "99").
    private static void AddRole(JsonElement element, List<RoleType> roles)
    {
        if (element.ValueKind == JsonValueKind.String &&
            Enum.TryParse(element.GetString(), ignoreCase: true, out RoleType role) &&
            Enum.IsDefined(role))
        {
            roles.Add(role);
        }
    }

    // base64url (RFC 7515): '-' e '_' al posto di '+' e '/', padding '=' omesso
    private static byte[] DecodeBase64Url(string segment)
    {
        string base64 = segment
            .Replace('-', '+')
            .Replace('_', '/');

        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');

        return Convert.FromBase64String(base64);
    }
}
