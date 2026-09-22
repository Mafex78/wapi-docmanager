using WAPIDocManager.UI.Features.Auth.Contracts;
using WAPIDocManager.UI.Features.Auth.Services;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Shared.Authentication;

/// <summary>
/// Sessione utente autenticato ottenuta dal login su WAPIIdentity
/// </summary>
/// <remarks>
/// <para>
/// Creata da <c>Features/Auth/Services/AuthApiService</c> e salvata come JSON nel sessionStorage
/// da <c>Shared/Authentication/SessionStorageUserSessionStore</c>: rinominare le proprietà rende illeggibili
/// le sessioni già salvate (l'utente deve solo rifare il login).
/// </para>
/// <para>
/// Da qui <c>JwtAuthenticationStateProvider</c> (progetto UI) costruisce il ClaimsPrincipal usato da AuthorizeView/[Authorize].
/// </para>
/// </remarks>
public record UserSession
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    /// <summary>JWT da inviare come <c>Authorization: Bearer</c> (vedi <c>Shared/Api/BearerTokenHandler</c>).</summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>Scadenza del token in UTC (<c>LoginResponse.TokenExpiration</c>, di default 60 minuti dal login).</summary>
    public DateTime ExpirationUtc { get; init; }

    /// <summary>Ruoli letti dal payload del JWT (non restituiti esplicitamente dalla risposta di login).</summary>
    public IReadOnlyList<RoleType> Roles { get; init; } = new List<RoleType>();

    /// <summary>Le API non hanno tolleranza sulla scadenza (<c>ClockSkew = TimeSpan.Zero</c>): alla scadenza il token è già rifiutato.</summary>
    public bool IsExpired(DateTime utcNow)
    {
        return utcNow >= ExpirationUtc;
    }

    public bool IsInRole(RoleType role)
    {
        return Roles.Contains(role);
    }
}
