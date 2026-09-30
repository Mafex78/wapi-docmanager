using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Shared.Api;

/// Registrazione dell'infrastruttura HTTP condivisa da tutte le funzionalità: indirizzi dei servizi, sessione
/// utente, sorgente del tempo e gestore del token.
/// Va chiamata PRIMA delle registrazioni degli slice, che dipendono dal gestore del token e dagli indirizzi base.
/// REGOLA DEI COMPONENTI CONDIVISI: qui entra solo ciò che DUE O PIÙ funzionalità usano davvero. Finché un tipo
/// serve a una sola funzionalità resta dentro la sua, e si promuove qui quando compare il secondo utilizzatore —
/// che è la norma, non un'eccezione. Il percorso inverso vale uguale: ciò che è rimasto usato da una sola
/// funzionalità torna dentro quella.
public static class ApiInfrastructureRegistration
{
    public static IServiceCollection AddApiInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(configuration.ReadApiOptions());

        // la sorgente del tempo è iniettata per rendere testabile il controllo di scadenza del token
        services.TryAddSingleton(TimeProvider.System);

        // Singleton: la stessa istanza deve servire i componenti e i gestori delle richieste HTTP, che vengono
        // risolti in uno scope di dipendenze separato
        services.AddSingleton<IUserSessionStore, SessionStorageUserSessionStore>();
        services.AddTransient<BearerTokenHandler>();

        return services;
    }

    /// Indirizzo base del servizio di autenticazione, con lo slash finale.
    public static Uri IdentityBaseAddress(this ApiOptions options)
    {
        return ToBaseAddress(options.IdentityBaseUrl);
    }

    /// Indirizzo base del servizio documenti, con lo slash finale.
    public static Uri DocumentBaseAddress(this ApiOptions options)
    {
        return ToBaseAddress(options.DocumentBaseUrl);
    }

    /// Legge la sezione di configurazione degli indirizzi.
    /// Serve anche alle registrazioni delle singole funzionalità, che devono fissare l'indirizzo base del proprio
    /// client HTTP al momento della registrazione, quando il contenitore delle dipendenze non è ancora costruito.
    public static ApiOptions ReadApiOptions(this IConfiguration configuration)
    {
        return configuration.GetSection(ApiOptions.SectionName).Get<ApiOptions>() ?? new ApiOptions();
    }

    // Lo slash finale è indispensabile: senza, un percorso relativo sostituirebbe l'ultimo segmento dell'indirizzo base.
    private static Uri ToBaseAddress(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException($"Missing API base url in configuration section '{ApiOptions.SectionName}'.");
        }

        return new Uri(url.EndsWith('/') ? url : $"{url}/");
    }
}
