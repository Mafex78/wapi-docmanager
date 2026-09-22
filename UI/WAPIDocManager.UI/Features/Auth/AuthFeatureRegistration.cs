using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WAPIDocManager.UI.Features.Auth.Contracts;
using WAPIDocManager.UI.Features.Auth.Models;
using WAPIDocManager.UI.Features.Auth.Services;
using WAPIDocManager.UI.Shared.Api;
using WAPIDocManager.UI.Shared.Authentication;
using WAPIDocManager.UI.Shared.Navigation;

namespace WAPIDocManager.UI.Features.Auth;

/// <summary>
/// Slice "Autenticazione": login dell'utente e creazione della sessione
/// </summary>
/// <remarks>
/// <para>
/// CONTENUTO DELLO SLICE
/// <list type="bullet">
///   <item><c>Pages/Login.razor</c> – unica pagina anonima dell'app ([AllowAnonymous]); rotta in <c>Shared/Navigation/AppRoutes</c>.</item>
///   <item><c>Models/LoginModel</c> – modello del form con DataAnnotations.</item>
///   <item><c>Services/IAuthService</c> + <c>AuthApiService</c> – chiamata a <c>POST /api/v1/auth/login</c> di WAPIIdentity
///         (endpoint anonimo: il typed HttpClient NON monta BearerTokenHandler); <c>JwtPayloadReader</c> legge i ruoli dal token.</item>
///   <item><c>Contracts/LoginRequest</c>, <c>LoginResponse</c> – forme di trasporto, con gli operatori di conversione.</item>
/// </list>
/// </para>
/// <para>
/// La sessione prodotta dal login (<c>UserSession</c>) NON sta in questo slice ma in <c>Shared/Authentication</c>:
/// la usano anche l'handler del token e lo stato di autenticazione, quindi è materiale condiviso.
/// </para>
/// </remarks>
public static class AuthFeatureRegistration
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services, IConfiguration configuration)
    {
        ApiOptions apiOptions = configuration.ReadApiOptions();

        // login: endpoint anonimo, nessun token da allegare
        services.AddHttpClient<IAuthService, AuthApiService>(client =>
            client.BaseAddress = apiOptions.IdentityBaseAddress());

        return services;
    }
}
