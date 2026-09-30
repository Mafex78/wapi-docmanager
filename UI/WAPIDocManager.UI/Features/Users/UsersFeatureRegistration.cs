using FluentValidation;
using WAPIDocManager.UI.Features.Users.Models;
using WAPIDocManager.UI.Features.Users.Validators;
using WAPIDocManager.UI.Features.Users.Services;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Users;

/// Slice "Utenti": registrazione di un nuovo utente, riservata a chi ha ruolo di amministratore.
/// Mappa dello slice:
///   UserRegister              form di registrazione, accessibile ai soli amministratori;
///   RegisterUserModel         modello del form, senza regole di validazione al suo interno;
///   RegisterUserModelValidator  le regole, risolte dalle dipendenze;
///   IUserService              la chiamata di registrazione; l'endpoint è protetto, quindi questo client HTTP monta
///                             il gestore che allega il token;
///   RegisterUserRequest,
///   RegisterUserResponse      forme di trasporto, copie lato client di quelle del servizio.
/// I ruoli assegnabili non stanno qui ma fra i componenti condivisi, perché servono anche ai permessi sui documenti
/// e alla barra superiore.
public static class UsersFeatureRegistration
{
    public static IServiceCollection AddUsersFeature(this IServiceCollection services, IConfiguration configuration)
    {
        ApiOptions apiOptions = configuration.ReadApiOptions();

        services.AddHttpClient<IUserService, UserApiService>(client =>
                client.BaseAddress = apiOptions.IdentityBaseAddress())
            .AddHttpMessageHandler<BearerTokenHandler>();

        // regole del form: risolte dalle dipendenze dal componente di validazione; senza stato, quindi Singleton
        services.AddSingleton<IValidator<RegisterUserModel>, RegisterUserModelValidator>();

        return services;
    }
}
