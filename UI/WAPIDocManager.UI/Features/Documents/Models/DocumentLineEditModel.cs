using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Shared.Validation;

namespace WAPIDocManager.UI.Features.Documents.Models;

/// <summary>
/// Riga documento in modifica (regole allineate a DocumentCreateUpdateRequestDocumentLineValidator)
/// </summary>
/// <remarks>
/// È una classe mutabile (non record) perché viene legata direttamente agli input di <c>Features/Documents/Views/Components/DocumentLinesEditor</c>
/// e l'istanza fa da chiave (<c>@key</c>) e da modello del FieldIdentifier per i messaggi di validazione.
/// </remarks>
public class DocumentLineEditModel
{
    public string? Description { get; set; }

    /// <summary>Valore iniziale 1 per le nuove righe.</summary>
    public decimal Quantity { get; set; } = 1M;
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Anteprima del totale riga (il valore definitivo è calcolato dal server)
    /// </summary>
    /// <remarks>Stesso calcolo del backend (<c>Math.Round</c> con arrotondamento bancario predefinito).</remarks>
    public decimal Total => Math.Round(Quantity * UnitPrice, 2);

    /// <summary>
    /// Riga del documento letto dalle API → riga del form (il totale è ricalcolato come anteprima).
    /// </summary>
    public static explicit operator DocumentLineEditModel(DocumentLine line)
    {
        return new DocumentLineEditModel
        {
            Description = line.Description,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice
        };
    }
}
