using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Auth.Services.Dto;

/// Risposta di POST api/v1/auth/login.
/// I ruoli non sono inclusi: si leggono dal payload del token.
public record LoginResponse
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;

    /// Scadenza in UTC, calcolata dal server sommando all'istante dell'accesso la durata configurata.
    public DateTime TokenExpiration { get; init; }

    /// Dalla risposta alla sessione dell'utente, con i ruoli estratti dal token.
    public static explicit operator UserSession(LoginResponse login)
    {
        return new UserSession
        {
            UserId = login.UserId,
            Email = login.Email,
            Token = login.Token,

            // il server invia la scadenza in UTC; se arrivasse senza fuso la si considera comunque UTC
            ExpirationUtc = login.TokenExpiration.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(login.TokenExpiration, DateTimeKind.Utc)
                : login.TokenExpiration.ToUniversalTime(),
            Roles = JwtPayloadReader.ReadRoles(login.Token)
        };
    }
}
