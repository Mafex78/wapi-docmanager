using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Contracts;

/// <summary>
/// Body di <c>PUT api/v1/documents/{id}</c> (<c>WAPIDocument.Application/Dto/Document/DocumentUpdateRequest.cs</c>).
/// </summary>
/// <remarks>
/// Sostituisce data, valuta, cliente e righe. Una lista righe vuota viene passata dal server come null al dominio.
/// Il tipo del documento non è modificabile. Se il DTO cambia sul server va aggiornato qui.
/// </remarks>
public record DocumentUpdateRequest
{
    /// <summary>Inviata come mezzanotte UTC ("yyyy-MM-ddT00:00:00Z"); il server ne usa solo anno/mese/giorno.</summary>
    public DateTime Date { get; init; }

    /// <summary>Sempre "EUR": se null il server azzererebbe la valuta del documento.</summary>
    public string? Currency { get; init; }

    public CustomerDto? Customer { get; init; }
    public IList<DocumentLineRequest> DocumentLines { get; init; } = new List<DocumentLineRequest>();

    /// <summary>
    /// Form → body di modifica (data a mezzanotte UTC, valuta sempre EUR).
    /// </summary>
    public static explicit operator DocumentUpdateRequest(DocumentEditModel model)
    {
        return new DocumentUpdateRequest
        {
            // WAPIDocument normalizza la data a mezzanotte UTC
            Date = DateTime.SpecifyKind(model.Date.Date, DateTimeKind.Utc),
            Currency = DocumentCurrency.Code,
            Customer = (CustomerDto)model.Customer,
            DocumentLines = model.Lines
                .Select(line => (DocumentLineRequest)line)
                .ToList()
        };
    }
}
