using Microsoft.Extensions.Logging;

namespace WAPIDocManager.UI.Shared.Navigation;

/// <summary>
/// Decide dove andare a partire da un indirizzo di ritorno preso dalla query string
/// </summary>
/// <remarks>
/// <para>
/// Mette insieme i tre casi possibili, che le pagine trattano allo stesso modo:
/// parametro assente → si usa il ripiego, senza rumore (è il caso normale: dal dettaglio documento non si passa
/// alcun returnUrl); parametro valido → si usa; parametro presente ma rifiutato → si usa il ripiego e si scrive un
/// avviso nella console del browser, perché in quel caso o il link è stato confezionato da qualcun altro, oppure
/// c'è un difetto nostro nella costruzione dei link — e senza traccia le due situazioni sono indistinguibili.
/// </para>
/// <para>
/// Il valore è controllato da chi costruisce il link: finisce in console, ma non va MAI reso nella pagina.
/// Questo controllo protegge l'utente dal finire fuori dall'applicazione; non è un confine di autorizzazione,
/// che resta negli attributi [Authorize] e nel token verificato dalle API.
/// </para>
/// <para>
/// È un servizio e non un metodo statico perché ha bisogno del logger, ed è iniettato dalle pagine che gestiscono
/// un returnUrl: <c>Features/Auth/Views/Pages/Login.razor</c> e
/// <c>Features/Documents/Views/Pages/DocumentEdit.razor</c>. Registrato in Program.cs.
/// </para>
/// </remarks>
public sealed partial class ReturnUrlResolver
{
    private readonly ILogger<ReturnUrlResolver> _logger;

    public ReturnUrlResolver(ILogger<ReturnUrlResolver> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Indirizzo a cui navigare: il candidato se utilizzabile, altrimenti <paramref name="fallbackUrl"/>.
    /// </summary>
    /// <param name="targetUrl">Valore arrivato dalla query string.</param>
    /// <param name="fallbackUrl">Destinazione predefinita della pagina.</param>
    /// <param name="forbiddenPrefixes">Percorsi da rifiutare anche se relativi (es. la pagina stessa).</param>
    public string Resolve(string? targetUrl, string fallbackUrl, params string[] forbiddenPrefixes)
    {
        if (string.IsNullOrWhiteSpace(targetUrl))
        {
            return fallbackUrl;
        }

        if (SafeReturnUrl.TryGetSafeRelativeUrl(targetUrl, out string? safeUrl, forbiddenPrefixes))
        {
            return safeUrl;
        }

        LogRejectedReturnUrl(_logger, targetUrl, fallbackUrl);

        return fallbackUrl;
    }

    // generato da [LoggerMessage]: evita l'allocazione a ogni chiamata (regola CA1848) e fissa il testo in un punto solo.
    // I messaggi di log si scrivono in inglese: il destinatario è chi sviluppa, non l'utente (i testi dell'utente stanno nei .resx).
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Return URL ignored because it is not a usable relative path: {ReturnUrl}. Navigating to {Fallback} instead.")]
    private static partial void LogRejectedReturnUrl(ILogger logger, string returnUrl, string fallback);
}
