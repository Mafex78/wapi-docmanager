using FluentValidation;
using WAPIDocManager.UI.Features.Auth.Models;
using WAPIDocManager.UI.Features.Auth.Validators;
using WAPIDocManager.UI.Features.Auth.Services;
using WAPIDocManager.UI.Shared.Api;

namespace WAPIDocManager.UI.Features.Auth;

/// Slice "Autenticazione": accesso dell'utente e creazione della sessione.
/// Mappa dello slice, da cui conviene partire per capirlo:
///   Login                 unica pagina anonima dell'applicazione;
///   LoginModel            modello del form, senza attributi di validazione: le regole stanno nel validatore;
///   LoginModelValidator   regole del form, risolte dalle dipendenze e applicate dal componente di validazione;
///   IAuthService          l'accesso vero e proprio, con la sessione che ne risulta;
///   JwtPayloadReader      estrae i ruoli dal payload del token;
///   LoginRequest,
///   LoginResponse         forme di trasporto, con gli operatori di conversione da e verso il modello.
/// Le forme di trasporto sono copie lato client di quelle del servizio: client e server non condividono progetti,
/// per tenere autonomi i due gruppi di lavoro. Se cambiano là, vanno aggiornate qui e nei test del servizio.
/// La sessione prodotta dall'accesso non sta in questo slice ma fra i componenti condivisi, perché la usano anche
/// il gestore del token e lo stato di autenticazione.
public static class AuthFeatureRegistration
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services, IConfiguration configuration)
    {
        ApiOptions apiOptions = configuration.ReadApiOptions();

        // accesso: endpoint anonimo, nessun token da allegare
        services.AddHttpClient<IAuthService, AuthApiService>(client =>
            client.BaseAddress = apiOptions.IdentityBaseAddress());

        // regole del form: risolte dalle dipendenze dal componente di validazione; senza stato, quindi Singleton
        services.AddSingleton<IValidator<LoginModel>, LoginModelValidator>();

        return services;
    }
}
