using Microsoft.Extensions.Logging;

namespace WAPIDocManager.UI.Shared.Navigation;

/// Decide dove andare a partire da un indirizzo di ritorno preso dalla query string.
/// Mette insieme i tre casi possibili, che le pagine trattano allo stesso modo: parametro assente, e si usa il
/// ripiego senza fare rumore, perché è il caso normale; parametro valido, e si usa; parametro presente ma
/// rifiutato, e allora si ripiega scrivendo un avviso nella console del browser — perché in quel caso o il
/// collegamento è stato confezionato da qualcun altro, oppure c'è un difetto nostro nel costruirlo, e senza traccia
/// le due situazioni sono indistinguibili.
/// Il valore lo controlla chi ha costruito il collegamento: finisce in console, ma non va MAI mostrato nella pagina.
/// Questo controllo protegge l'utente dal finire fuori dall'applicazione, e non è un confine di autorizzazione:
/// quello resta negli attributi delle pagine e nel token verificato dalle API.
/// È un servizio e non un metodo statico perché ha bisogno del registro dei messaggi.
public sealed partial class ReturnUrlResolver
{
    private readonly ILogger<ReturnUrlResolver> _logger;

    public ReturnUrlResolver(ILogger<ReturnUrlResolver> logger)
    {
        _logger = logger;
    }

    /// Indirizzo a cui navigare: il candidato se utilizzabile, altrimenti la destinazione di ripiego della pagina.
    /// I prefissi vietati sono percorsi da rifiutare anche quando sono relativi, tipicamente la pagina stessa.
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

    // generato dall'attributo: evita l'allocazione a ogni chiamata e fissa il testo in un punto solo.
    // I messaggi di log si scrivono in inglese: il destinatario è chi sviluppa, non l'utente.
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Return URL ignored because it is not a usable relative path: {ReturnUrl}. Navigating to {Fallback} instead.")]
    private static partial void LogRejectedReturnUrl(ILogger logger, string returnUrl, string fallback);
}
