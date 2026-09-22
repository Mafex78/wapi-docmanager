using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;
using WAPIDocManager.UI.Features.Documents.Services;

namespace WAPIDocManager.UI.Features.Documents.Contracts;

/// <summary>
/// Riga in creazione/modifica (<c>WAPIDocument.Application/Dto/Document/DocumentCreateUpdateRequestDocumentLine.cs</c>).
/// </summary>
/// <remarks>
/// Il totale non viene inviato: lo calcola il server. Se il DTO cambia sul server va aggiornato qui.
/// </remarks>
public record DocumentLineRequest
{
    public string? Description { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// Riga del form → riga del body (descrizione vuota a null, altrimenti con trim).
    /// </summary>
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
