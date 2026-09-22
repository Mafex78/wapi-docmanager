using WAPIDocManager.UI.Features.Auth.Services;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Auth.Contracts;

/// <summary>
/// Risposta del login (<c>WAPIIdentity.Application/Dto/LoginResponse.cs</c>).
/// </summary>
/// <remarks>
/// I ruoli non sono inclusi: si leggono dal payload di <see cref="Token"/> (Services/JwtPayloadReader).
/// Se il DTO cambia sul server va aggiornato qui e nei test di AuthApiService.
/// </remarks>
public record LoginResponse
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;

    /// <summary>Scadenza in UTC (il server usa DateTime.UtcNow + Jwt:ExpirationMinutes).</summary>
    public DateTime TokenExpiration { get; init; }

    /// <summary>
    /// Risposta del login → sessione utente del client (ruoli letti dal JWT).
    /// </summary>
    public static explicit operator UserSession(LoginResponse login)
    {
        return new UserSession
        {
            UserId = login.UserId,
            Email = login.Email,
            Token = login.Token,

            // il server invia la scadenza in UTC ("...Z"); se arrivasse senza fuso la si considera comunque UTC
            ExpirationUtc = login.TokenExpiration.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(login.TokenExpiration, DateTimeKind.Utc)
                : login.TokenExpiration.ToUniversalTime(),
            Roles = JwtPayloadReader.ReadRoles(login.Token)
        };
    }
}
