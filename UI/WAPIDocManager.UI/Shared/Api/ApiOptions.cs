using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Shared.Api;

/// <summary>
/// Indirizzi base dei microservizi (sezione "Api" di wwwroot/appsettings.json)
/// </summary>
/// <remarks>
/// <para>
/// In Blazor WebAssembly la configurazione è scaricata dal browser da <c>WAPIDocManager.UI/wwwroot/appsettings.json</c>
/// (eventualmente <c>appsettings.{Environment}.json</c>): non contiene segreti. I file JSON non ammettono commenti, per questo è documentata qui.
/// Valori di sviluppo: Identity <c>https://localhost:7205/</c>, Document <c>https://localhost:7273/</c> (launchSettings delle API).
/// </para>
/// <para>
/// Ogni origine da cui gira il client (sviluppo: <c>https://localhost:7150</c> e <c>http://localhost:5150</c>, vedi
/// <c>WAPIDocManager.UI/Properties/launchSettings.json</c>) deve essere presente in <c>Cors:AllowedOrigins</c> di
/// ENTRAMBE le API (<c>WAPIIdentity/appsettings.json</c>, <c>WAPIDocument/appsettings.json</c>), altrimenti il browser
/// blocca le chiamate (in UI appare come HttpRequestException, messaggio "servizio non raggiungibile").
/// </para>
/// </remarks>
public class ApiOptions
{
    public const string SectionName = "Api";

    /// <summary>URL base di WAPIIdentity (lo slash finale viene aggiunto se mancante).</summary>
    public string IdentityBaseUrl { get; set; } = string.Empty;

    /// <summary>URL base di WAPIDocument (lo slash finale viene aggiunto se mancante).</summary>
    public string DocumentBaseUrl { get; set; } = string.Empty;
}
