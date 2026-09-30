using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Services.Dto;

/// Riga di documento in creazione e in modifica.
/// Il totale non viene inviato: lo calcola il server, ed è l'unico valore di cui fa fede.
public record DocumentLineRequest
{
    public string? Description { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }

    /// Dalla riga del form alla riga da inviare: descrizione vuota azzerata, altrimenti ripulita dagli spazi.
    public static explicit operator DocumentLineRequest(DocumentLineEditModel line)
    {
        return new DocumentLineRequest
        {
            Description = DocumentMapper.Normalize(line.Description),
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice
        };
    }
}
