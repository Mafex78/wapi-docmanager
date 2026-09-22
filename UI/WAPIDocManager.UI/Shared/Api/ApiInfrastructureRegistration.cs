using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Shared.Authentication;

namespace WAPIDocManager.UI.Shared.Api;

/// <summary>
/// Registrazione DI dell'infrastruttura HTTP condivisa da tutte le funzionalità
/// </summary>
/// <remarks>
/// <para>
/// Contenuto di <c>Shared/Api</c>: <see cref="ApiOptions"/> (URL base delle due API, sezione "Api" di
/// wwwroot/appsettings.json), <see cref="BearerTokenHandler"/> (header Authorization e logout su token scaduto/401),
/// <see cref="ApiResponseReader"/> (risposte non 2xx → <see cref="ApiException"/>), <see cref="JsonDefaults"/> e
/// <see cref="ProblemDetailsDto"/>.
/// </para>
/// <para>
/// REGOLA DI Shared/: qui entra solo ciò che DUE O PIÙ slice usano davvero. Finché un tipo serve a una sola
/// funzionalità resta dentro il suo slice; si promuove qui quando compare il secondo utilizzatore (ed è la norma,
/// non un'eccezione). Il percorso inverso — riportare in uno slice ciò che è rimasto usato da uno solo — vale uguale.
/// </para>
/// <para>
/// Chiamata da <c>Program.cs</c> PRIMA delle registrazioni degli slice, che dipendono da
/// <see cref="BearerTokenHandler"/> e dagli URL base.
/// </para>
/// </remarks>
public static class ApiInfrastructureRegistration
{
    /// <summary>Registra opzioni delle API, sessione utente, TimeProvider e handler del token.</summary>
    public static IServiceCollection AddApiInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(configuration.ReadApiOptions());

        // TimeProvider iniettato per rendere testabile il controllo di scadenza del token
        services.TryAddSingleton(TimeProvider.System);

        // Singleton: stessa istanza per componenti e DelegatingHandler (risolti da IHttpClientFactory in uno scope separato)
        services.AddSingleton<IUserSessionStore, SessionStorageUserSessionStore>();
        services.AddTransient<BearerTokenHandler>();

        return services;
    }

    /// <summary>
    /// URL base dell'API Identity, con lo slash finale.
    /// </summary>
    /// <remarks>Usato dalle registrazioni degli slice Auth e Users.</remarks>
    public static Uri IdentityBaseAddress(this ApiOptions options)
    {
        return ToBaseAddress(options.IdentityBaseUrl);
    }

    /// <summary>URL base dell'API Document, con lo slash finale.</summary>
    public static Uri DocumentBaseAddress(this ApiOptions options)
    {
        return ToBaseAddress(options.DocumentBaseUrl);
    }

    /// <summary>
    /// Legge la sezione "Api" della configurazione.
    /// </summary>
    /// <remarks>
    /// Usata anche dalle registrazioni degli slice, che devono impostare il BaseAddress del proprio typed HttpClient
    /// al momento della registrazione (prima che il container sia costruito).
    /// </remarks>
    public static ApiOptions ReadApiOptions(this IConfiguration configuration)
    {
        return configuration.GetSection(ApiOptions.SectionName).Get<ApiOptions>() ?? new ApiOptions();
    }

    // Lo slash finale è indispensabile: senza, i percorsi relativi ("api/v1/...") sostituirebbero l'ultimo segmento dell'URL base.
    private static Uri ToBaseAddress(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException($"Missing API base url in configuration section '{ApiOptions.SectionName}'.");
        }

        return new Uri(url.EndsWith('/') ? url : $"{url}/");
    }
}
