using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WAPIDocManager.UI.Features.Users.Contracts;
using WAPIDocManager.UI.Features.Users.Models;
using WAPIDocManager.UI.Features.Users.Services;
using WAPIDocManager.UI.Shared.Api;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Features.Users;

/// <summary>
/// Slice "Utenti": registrazione di un nuovo utente, riservata agli Admin
/// </summary>
/// <remarks>
/// <para>
/// CONTENUTO DELLO SLICE
/// <list type="bullet">
///   <item><c>Pages/UserRegister.razor</c> – form di registrazione, protetto da [Authorize(Roles = AppRoles.Admin)].</item>
///   <item><c>Models/RegisterUserModel</c> – modello del form (email, password, ruoli).</item>
///   <item><c>Services/IUserService</c> + <c>UserApiService</c> – <c>POST /api/v1/users/register</c> di WAPIIdentity
///         (endpoint protetto: il typed HttpClient monta BearerTokenHandler).</item>
///   <item><c>Contracts/RegisterUserRequest</c>, <c>RegisterUserResponse</c> – forme di trasporto.</item>
/// </list>
/// </para>
/// <para>
/// I ruoli assegnabili sono <c>RoleType</c>, che vive in <c>Shared/Authentication</c> perché serve anche ai permessi
/// dei documenti e alla barra superiore.
/// </para>
/// </remarks>
public static class UsersFeatureRegistration
{
    public static IServiceCollection AddUsersFeature(this IServiceCollection services, IConfiguration configuration)
    {
        ApiOptions apiOptions = configuration.ReadApiOptions();

        services.AddHttpClient<IUserService, UserApiService>(client =>
                client.BaseAddress = apiOptions.IdentityBaseAddress())
            .AddHttpMessageHandler<BearerTokenHandler>();

        return services;
    }
}
