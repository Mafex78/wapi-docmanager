using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Services.Dto;

/// Riga di documento come arriva nelle risposte.
public record DocumentLineDto
{
    public string? Description { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }

    /// Totale di riga calcolato dal server.
    public decimal Total { get; init; }

    /// Dalla risposta alla riga dell'applicazione, totale compreso.
    public static explicit operator DocumentLine(DocumentLineDto line)
    {
        return new DocumentLine
        {
            Description = line.Description,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            Total = line.Total
        };
    }
}
