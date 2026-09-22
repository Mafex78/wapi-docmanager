using WAPIDocManager.UI.Features.Documents.Entities;

namespace WAPIDocManager.UI.Features.Documents.Contracts;

/// <summary>
/// Riga nelle risposte (<c>WAPIDocument.Application/Dto/Document/DocumentLineDto.cs</c>).
/// </summary>
/// <remarks>Se il DTO cambia sul server va aggiornato qui.</remarks>
public record DocumentLineDto
{
    public string? Description { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }

    /// <summary>Totale riga calcolato dal server.</summary>
    public decimal Total { get; init; }

    /// <summary>
    /// DTO → riga del documento del client (totale compreso).
    /// </summary>
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
