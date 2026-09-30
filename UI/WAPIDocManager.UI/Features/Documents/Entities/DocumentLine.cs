namespace WAPIDocManager.UI.Features.Documents.Entities;

/// Riga di un documento letto dalle API, in sola lettura. Per la riga in corso di modifica dentro un form esiste un
/// modello a parte, mutabile.
public record DocumentLine
{
    public string? Description { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }

    /// Totale di riga calcolato dal server: quantità per prezzo, arrotondato a due decimali.
    public decimal Total { get; init; }

    /// Stessa regola applicata dal server: descrizione presente, quantità e prezzo maggiori di zero.
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Description) &&
               Quantity > 0 &&
               UnitPrice > 0;
    }
}
