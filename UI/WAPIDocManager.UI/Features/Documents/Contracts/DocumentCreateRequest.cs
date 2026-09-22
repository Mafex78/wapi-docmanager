using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Contracts;

/// <summary>
/// Body di <c>POST api/v1/documents</c> (<c>WAPIDocument.Application/Dto/Document/DocumentCreateRequest.cs</c>).
/// </summary>
/// <remarks>
/// Senza data: il server imposta la data odierna, lo stato Draft e il numero (GUID).
/// Se il DTO cambia sul server va aggiornato qui e nei test <c>DocumentApiServiceTests</c>.
/// </remarks>
public record DocumentCreateRequest
{
    public DocumentType Type { get; init; }

    /// <summary>Sempre "EUR" dal client (vedi <c>DocumentCurrency</c>).</summary>
    public string? Currency { get; init; }

    public CustomerDto? Customer { get; init; }
    public IList<DocumentLineRequest> DocumentLines { get; init; } = new List<DocumentLineRequest>();

    /// <summary>
    /// Form → body di creazione (valuta sempre EUR, vedi <see cref="DocumentCurrency"/>).
    /// </summary>
    public static explicit operator DocumentCreateRequest(DocumentEditModel model)
    {
        return new DocumentCreateRequest
        {
            Type = model.Type,
            Currency = DocumentCurrency.Code,
            Customer = (CustomerDto)model.Customer,
            DocumentLines = model.Lines
                .Select(line => (DocumentLineRequest)line)
                .ToList()
        };
    }
}
