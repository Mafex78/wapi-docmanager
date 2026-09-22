using System.Net;

namespace WAPIDocManager.UI.Shared.Api;

/// <summary>
/// Errore restituito da una chiamata alle API (status code + detail del ProblemDetails)
/// </summary>
/// <remarks>
/// <para>
/// Sollevata da <c>Shared/Api/ApiResponseReader</c> per ogni risposta non 2xx e da
/// <c>Shared/Api/BearerTokenHandler</c> (401) quando il token è già scaduto.
/// </para>
/// <para>
/// Mappatura degli status code lato server (<c>Shared.Application/GlobalExceptionHandler.cs</c>):
/// 400 = ArgumentException / ValidationException / InvalidOperationException (detail = messaggio, in inglese);
/// 401 = UnauthorizedAccessException (es. credenziali errate) o JWT non valido;
/// 403 = ruolo non autorizzato; 404 = KeyNotFoundException; 500 = altro (detail vuoto).
/// </para>
/// <para>
/// Nella UI il componente <c>Shared/Components/ApiErrorAlert</c> sceglie il titolo localizzato in base a <see cref="StatusCode"/>
/// e mostra <see cref="Detail"/> così com'è. Le pagine intercettano sempre <c>ApiException</c> e <c>HttpRequestException</c>
/// (quest'ultima = API non raggiungibili o bloccate da CORS).
/// </para>
/// </remarks>
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

    /// <summary>Messaggio restituito dal server (null se assente o vuoto).</summary>
    public string? Detail { get; }
}
