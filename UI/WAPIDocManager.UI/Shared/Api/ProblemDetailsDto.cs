namespace WAPIDocManager.UI.Shared.Api;

/// Corpo delle risposte di errore, nel formato application/problem+json.
/// Il server lo produce in due modi: la gestione centralizzata delle eccezioni mette il messaggio nel dettaglio e
/// lascia un titolo generico; la validazione automatica dei parametri in ingresso mette invece i messaggi
/// nell'elenco degli errori, raggruppati per campo.
public record ProblemDetailsDto
{
    public string? Type { get; init; }
    public string? Title { get; init; }
    public int? Status { get; init; }
    public string? Detail { get; init; }

    /// Valorizzato solo quando l'errore viene dalla validazione dei parametri in ingresso.
    public Dictionary<string, string[]>? Errors { get; init; }
}
