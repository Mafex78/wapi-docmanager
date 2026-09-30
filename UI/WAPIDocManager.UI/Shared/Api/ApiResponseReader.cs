using System.Net.Http.Json;
using System.Text.Json;

namespace WAPIDocManager.UI.Shared.Api;

/// Lettura delle risposte HTTP: deserializza quelle riuscite e trasforma gli errori in eccezione.
/// È il punto unico di gestione degli errori HTTP: ogni nuovo metodo dei servizi deve passare da qui, con la lettura
/// completa quando la risposta ha un body, o con la sola verifica dello status quando non ce l'ha.
public static class ApiResponseReader
{
    /// Verifica lo status e deserializza il body. Su una risposta riuscita un body vuoto è considerato un errore.
    public static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken);

        T? result = await response.Content.ReadFromJsonAsync<T>(JsonDefaults.Options, cancellationToken);

        return result ?? throw new ApiException(response.StatusCode, "Empty response body.");
    }

    /// Solleva l'eccezione, con il messaggio del server se presente, per ogni status non riuscito.
    public static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string? detail = await TryReadDetailAsync(response, cancellationToken);

        throw new ApiException(response.StatusCode, detail);
    }

    // Estrae il messaggio: il dettaglio dell'errore, oppure gli errori di validazione concatenati.
    // Body non JSON o malformato = nessun messaggio: un errore di lettura non deve mai mascherare lo status originale.
    private static async Task<string?> TryReadDetailAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string? mediaType = response.Content.Headers.ContentType?.MediaType;

        // copre sia application/json sia application/problem+json
        if (mediaType is null || !mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        try
        {
            ProblemDetailsDto? problemDetails = await response.Content
                .ReadFromJsonAsync<ProblemDetailsDto>(JsonDefaults.Options, cancellationToken);

            if (problemDetails is null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(problemDetails.Detail))
            {
                return problemDetails.Detail;
            }

            if (problemDetails.Errors is { Count: > 0 })
            {
                return string.Join(Environment.NewLine, problemDetails.Errors.SelectMany(error => error.Value));
            }

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
