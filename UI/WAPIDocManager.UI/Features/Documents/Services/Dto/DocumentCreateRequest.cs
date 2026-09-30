using WAPIDocManager.UI.Features.Documents.Entities;
using WAPIDocManager.UI.Features.Documents.Models;

namespace WAPIDocManager.UI.Features.Documents.Services.Dto;

/// Body di POST api/v1/documents.
/// Non porta la data: sono il server ad assegnare data odierna, stato iniziale e numero.
public record DocumentCreateRequest
{
    public DocumentType Type { get; init; }

    /// Il client invia sempre la stessa valuta.
    public string? Currency { get; init; }

    public CustomerDto? Customer { get; init; }
    public IList<DocumentLineRequest> DocumentLines { get; init; } = new List<DocumentLineRequest>();

    /// Dal form al corpo della creazione.
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
