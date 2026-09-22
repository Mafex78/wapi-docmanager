using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Entities;

/// <summary>
/// Riga di un documento letto dalle API (sola lettura).
/// </summary>
/// <remarks>
/// Per la riga in modifica nei form si usa <c>Features/Documents/Models/DocumentLineEditModel</c>.
/// </remarks>
public record DocumentLine
{
    public string? Description { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// Totale riga calcolato dal server
    /// </summary>
    /// <remarks>Backend: <c>Math.Round(Quantity * UnitPrice, 2)</c> in <c>WAPIDocument.Domain/Entities/Documents/DocumentLine.cs</c>.</remarks>
    public decimal Total { get; init; }

    /// <summary>
    /// Stessa regola di <c>DocumentLine.IsValid()</c> del backend: descrizione presente, quantità e prezzo maggiori di zero.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Description) &&
               Quantity > 0 &&
               UnitPrice > 0;
    }
}
