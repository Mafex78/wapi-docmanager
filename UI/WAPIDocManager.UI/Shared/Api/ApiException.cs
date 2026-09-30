using System.Net;

namespace WAPIDocManager.UI.Shared.Api;

/// Errore restituito da una chiamata alle API: lo status e il messaggio del server.
/// Viene sollevata per ogni risposta non riuscita, e anche prima di partire quando il token è già scaduto.
/// Significato degli status, come li produce il server: 400 argomento non valido, validazione fallita oppure
/// operazione non ammessa nello stato corrente, con il messaggio in chiaro (in inglese); 401 credenziali errate o
/// token non valido; 403 ruolo non autorizzato; 404 risorsa inesistente; 500 tutto il resto, senza messaggio.
/// Le pagine intercettano sempre questa eccezione insieme a quella di rete: la seconda significa API non
/// raggiungibili, oppure richiesta bloccata dal controllo di origine del browser.
public class ApiException : Exception
{
    public ApiException(HttpStatusCode statusCode, string? detail)
        : base(string.IsNullOrWhiteSpace(detail)
            ? $"API request failed with status code {(int)statusCode}."
            : detail)
    {
        StatusCode = statusCode;
        Detail = string.IsNullOrWhiteSpace(detail) ? null : detail;
    }

    public HttpStatusCode StatusCode { get; }

    /// Messaggio restituito dal server, null se assente o vuoto.
    public string? Detail { get; }
}
