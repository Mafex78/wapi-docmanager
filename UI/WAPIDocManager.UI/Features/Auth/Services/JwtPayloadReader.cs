using System.Text.Json;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Auth.Services;

/// Legge i ruoli dal payload del token, senza verificarne la firma: quella la verifica il server a ogni chiamata.
/// Il token porta un claim di ruolo per ciascun ruolo dell'utente, scritto nel payload con il nome breve "role":
/// una stringa sola se il ruolo è uno, un array se sono più d'uno. Per prudenza si accetta anche il nome lungo.
/// La lettura è fatta a mano, con base64url e il lettore JSON di sistema, per non aggiungere al bundle WebAssembly
/// l'intera libreria dei token: qui i ruoli servono solo a decidere cosa mostrare, e un token manomesso verrebbe
/// comunque rifiutato dalle API.
public static class JwtPayloadReader
{
    private const string RoleClaim = "role";
    private const string RoleClaimLongName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    /// Ruoli noti presenti nel token, senza duplicati. Token malformato e ruoli sconosciuti vengono ignorati.
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

    // Il claim contiene il NOME del ruolo, non il suo numero.
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
