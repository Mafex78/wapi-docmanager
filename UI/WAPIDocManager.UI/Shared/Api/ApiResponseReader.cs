using System.Net.Http.Json;
using System.Text.Json;

namespace WAPIDocManager.UI.Shared.Api;

/// <summary>
/// Lettura delle risposte HTTP: deserializza i 2xx e converte gli errori in <see cref="ApiException"/>
/// </summary>
/// <remarks>
/// Punto unico di gestione degli errori HTTP per tutti i servizi API: ogni nuovo metodo dei servizi deve passare da qui
/// (<see cref="ReadAsync{T}"/> se la risposta ha un body, <see cref="EnsureSuccessAsync"/> altrimenti, es. DELETE 204).
/// </remarks>
public static class ApiResponseReader
{
    /// <summary>Verifica lo status e deserializza il body; un body vuoto su 2xx è considerato errore.</summary>
    public static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken);

        T? result = await response.Content.ReadFromJsonAsync<T>(JsonDefaults.Options, cancellationToken);

        return result ?? throw new ApiException(response.StatusCode, "Empty response body.");
    }

    /// <summary>Solleva <see cref="ApiException"/> (con il detail del ProblemDetails, se presente) per ogni status non 2xx.</summary>
    public static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string? detail = await TryReadDetailAsync(response, cancellationToken);

        throw new ApiException(response.StatusCode, detail);
    }

    // Estrae il messaggio: Detail (GlobalExceptionHandler) oppure gli Errors concatenati (ValidationProblemDetails).
    // Body non JSON o malformato → null: l'errore non deve mai mascherare lo status code originale.
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
